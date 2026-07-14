using Drova_Modding_API.Access;
using Il2CppDrova;
using Il2CppDrova.MapSystem;
using UnityEngine;

namespace DrovaMinimapMod
{
    /// <summary>
    /// Selects the enabled active-scene map whose native marker space contains the
    /// player's feet. A local map wins over overlapping world maps; the candidate
    /// itself owns the ordering rules within either category.
    /// </summary>
    internal sealed class PlayerPositionMapResolver
    {
        internal MapData? TryResolve(Vector2 playerWorldPosition)
        {
            if (!ProviderAccess.TryGetPlayerMetaDataGameHandler(out PlayerMetaDataGameHandler? playerMetadata)
                || playerMetadata == null)
            {
                return null;
            }

            MapCollection? mapCollection = playerMetadata.GetMapCollection();
            if (mapCollection == null)
            {
                return null;
            }

            MapCandidate? bestLocalMap = null;
            MapCandidate? bestWorldMap = null;
            foreach (MapData mapData in mapCollection.GetMapDataList())
            {
                if (!MapCandidate.TryCreate(mapData, playerWorldPosition, out MapCandidate candidate))
                {
                    continue;
                }

                if (candidate.IsWorldMap)
                {
                    if (candidate.IsPreferredTo(bestWorldMap))
                    {
                        bestWorldMap = candidate;
                    }
                }
                else if (candidate.IsPreferredTo(bestLocalMap))
                {
                    bestLocalMap = candidate;
                }
            }

            return bestLocalMap?.MapData ?? bestWorldMap?.MapData;
        }

        private enum MapScope
        {
            Local,
            World,
        }

        private enum WorldTerrain
        {
            Detailed,
            Rough,
            Supplemental,
        }

        /// <summary>
        /// A map definition that is valid for the player's current position. Its
        /// scope, terrain kind, and size are captured together so selection rules
        /// cannot become detached from the values they compare.
        /// </summary>
        private readonly struct MapCandidate
        {
            private const float AreaTieEpsilon = 0.01f;
            private const string WorldDefinitionPrefix = "MapDefinition_World";
            private const string DetailedTerrainToken = "World_Detailed";
            private const string RoughTerrainToken = "World_Rough";
            private const string ShrinesTerrainToken = "World_Shrines";

            private readonly MapScope _scope;
            private readonly WorldTerrain _worldTerrain;
            private readonly float _worldArea;

            private MapCandidate(MapData mapData, MapDefinition definition)
            {
                MapData = mapData;
                _scope = IsWorldDefinition(definition.name) ? MapScope.World : MapScope.Local;
                _worldTerrain = ClassifyWorldTerrain(definition.name);

                Vector2 size = definition.WorldMax - definition.WorldMin;
                _worldArea = Mathf.Abs(size.x * size.y);
            }

            internal MapData MapData { get; }
            internal bool IsWorldMap => _scope == MapScope.World;

            internal static bool TryCreate(
                MapData? mapData,
                Vector2 playerWorldPosition,
                out MapCandidate candidate)
            {
                if (mapData?.Definition == null)
                {
                    candidate = default;
                    return false;
                }

                MapDefinition definition = mapData.Definition;
                if (!mapData.IsEnabled
                    || !definition.IsValidOnActiveScene()
                    || !definition.IsWorldPosOnMap(playerWorldPosition))
                {
                    candidate = default;
                    return false;
                }

                candidate = new MapCandidate(mapData, definition);
                return true;
            }

            internal bool IsPreferredTo(MapCandidate? currentBest)
            {
                if (!currentBest.HasValue)
                {
                    return true;
                }

                MapCandidate current = currentBest.Value;
                return IsWorldMap
                    ? IsPreferredWorldTerrainTo(current)
                    : IsMoreSpecificThan(current);
            }

            private bool IsMoreSpecificThan(MapCandidate other)
            {
                return _worldArea < other._worldArea - AreaTieEpsilon;
            }

            private bool IsPreferredWorldTerrainTo(MapCandidate other)
            {
                int terrainComparison = _worldTerrain.CompareTo(other._worldTerrain);
                return terrainComparison < 0
                       || (terrainComparison == 0
                           && _worldArea < other._worldArea - AreaTieEpsilon);
            }

            private static bool IsWorldDefinition(string definitionName)
            {
                return definitionName.StartsWith(WorldDefinitionPrefix, StringComparison.OrdinalIgnoreCase);
            }

            private static WorldTerrain ClassifyWorldTerrain(string definitionName)
            {
                if (definitionName.Contains(DetailedTerrainToken, StringComparison.OrdinalIgnoreCase))
                {
                    return WorldTerrain.Detailed;
                }

                if (definitionName.Equals("MapDefinition_World", StringComparison.OrdinalIgnoreCase)
                    || definitionName.Contains(RoughTerrainToken, StringComparison.OrdinalIgnoreCase))
                {
                    return WorldTerrain.Rough;
                }

                return definitionName.Contains(ShrinesTerrainToken, StringComparison.OrdinalIgnoreCase)
                       || definitionName.Contains("WorldShrines", StringComparison.OrdinalIgnoreCase)
                    ? WorldTerrain.Supplemental
                    : WorldTerrain.Rough;
            }
        }
    }
}
