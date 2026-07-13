using Drova_Modding_API.Access;
using Il2CppDrova;
using Il2CppDrova.MapSystem;
using UnityEngine;

namespace DrovaMinimapMod
{
    /// <summary>
    /// Resolves the single enabled main-world map that contains the player.
    /// </summary>
    internal sealed class MainWorldMapResolver
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

            MapData? activeMap = mapCollection.GetMapDataForActiveScene();
            MapData? bestMatch = null;
            int bestPriority = int.MinValue;
            float smallestArea = float.MaxValue;
            var maps = mapCollection.GetMapDataList();
            for (int i = 0; i < maps.Count; i++)
            {
                MapData candidate = maps[i];
                if (candidate == null
                    || !candidate.IsEnabled
                    || !candidate.Definition.IsWorldPosOnMap(playerWorldPosition))
                {
                    continue;
                }

                // A containing cave or independent map takes precedence over the
                // overlapping world map and intentionally suppresses the minimap.
                if (!MinimapCompatibility.IsMainWorldDefinition(candidate.Definition.name))
                {
                    return null;
                }

                Vector2 size = candidate.Definition.WorldMax - candidate.Definition.WorldMin;
                float area = Mathf.Abs(size.x * size.y);
                int priority = GetPriority(candidate, activeMap);
                if (priority > bestPriority || (priority == bestPriority && area < smallestArea))
                {
                    bestPriority = priority;
                    smallestArea = area;
                    bestMatch = candidate;
                }
            }

            if (bestMatch != null)
            {
                return bestMatch;
            }

            return activeMap != null
                && activeMap.IsEnabled
                && MinimapCompatibility.IsMainWorldDefinition(activeMap.Definition.name)
                ? activeMap
                : null;
        }

        private static int GetPriority(MapData candidate, MapData? activeMap)
        {
            if (MinimapCompatibility.IsDetailedMainWorldDefinition(candidate.Definition.name))
            {
                return 20;
            }

            return candidate == activeMap ? 10 : 0;
        }
    }
}
