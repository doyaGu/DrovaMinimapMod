using Drova_Modding_API.Access;
using Drova_Modding_API.Systems;
using Il2CppDrova;
using Il2CppDrova.MapSystem;
using UnityEngine;

namespace DrovaMinimapMod
{
    internal sealed class MinimapController
    {
        private const string MainWorldDefinitionName = "MapDefinition_World";
        private const string DetailedMainWorldDefinitionName = "MapDefinition_World_Detailed";

        private readonly List<Region> _activeRegions = [];
        private Actor? _player;
        private AreaNameSystem? _areaNameSystem;
        private MinimapView? _view;

        public void OnPlayerFound(Actor player)
        {
            _player = player;
            _activeRegions.Clear();
            _view?.Dispose();
            _view = null;
            RefreshAreaSubscription();
        }

        public void RefreshAreaSubscription()
        {
            AreaNameSystem? current = AreaNameSystem.Instance;
            if (ReferenceEquals(_areaNameSystem, current))
            {
                return;
            }

            if (_areaNameSystem != null)
            {
                _areaNameSystem.OnRegionChanged -= OnRegionChanged;
            }

            _areaNameSystem = current;
            if (_areaNameSystem != null)
            {
                _areaNameSystem.OnRegionChanged += OnRegionChanged;
            }
        }

        public void Tick(MinimapSettings settings)
        {
            RefreshAreaSubscription();

            if (_player == null || !_player)
            {
                HideView();
                return;
            }

            GUIGameHandler? guiHandler = ProviderAccess.GetGUIGameHandler();
            if (guiHandler == null || guiHandler.GUIRoot == null)
            {
                HideView();
                return;
            }

            _view ??= new MinimapView(guiHandler.GUIRoot);
            if (!_view.IsAttachedTo(guiHandler.GUIRoot))
            {
                _view.Dispose();
                _view = new MinimapView(guiHandler.GUIRoot);
            }

            Vector3 position = _player.transform.position;
            Vector2 playerWorldPosition = new(position.x, position.y);
            MapData? mapData = GetBestMapData(playerWorldPosition);
            if (mapData == null)
            {
                _view.SetVisible(false);
                return;
            }

            if (!mapData.IsEnabled)
            {
                _view.SetVisible(false);
                return;
            }

            if (!IsMainWorldMap(mapData))
            {
                _view.SetVisible(false);
                return;
            }

            bool guiSuppressed = guiHandler.GUIIsHidden
                                 || guiHandler.HUDIsHidden
                                 || guiHandler.IsPlayerGameMenuWindowVisible()
                                 || guiHandler.HasOpenModalWindows();

            Vector2 lookDirection = _player.GetLookModule()?.CurrentLookDir ?? Vector2.up;
            _view.UpdateMap(mapData, playerWorldPosition, lookDirection, GetRegionLabel(), settings);

            _view.SetVisible(settings.Enabled && !guiSuppressed);
        }

        public void Dispose()
        {
            if (_areaNameSystem != null)
            {
                _areaNameSystem.OnRegionChanged -= OnRegionChanged;
                _areaNameSystem = null;
            }

            _view?.Dispose();
            _view = null;
            _player = null;
            _activeRegions.Clear();
        }

        private MapData? GetBestMapData(Vector2 playerWorldPosition)
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

            // The active scene map is often the broad World definition even while the
            // player is inside a smaller unlocked regional map. Prefer the smallest
            // enabled map which actually contains the player, then fall back to it.
            MapData? fallback = mapCollection.GetMapDataForActiveScene();
            MapData? bestMatch = null;
            int bestPriority = int.MinValue;
            float smallestArea = float.MaxValue;
            var maps = mapCollection.GetMapDataList();
            for (int i = 0; i < maps.Count; i++)
            {
                MapData candidate = maps[i];
                if (candidate == null)
                {
                    continue;
                }

                MapDefinition definition = candidate.Definition;
                bool containsPlayer = definition.IsWorldPosOnMap(playerWorldPosition);
                if (!candidate.IsEnabled || !containsPlayer)
                {
                    continue;
                }

                Vector2 size = candidate.Definition.WorldMax - candidate.Definition.WorldMin;
                float area = Mathf.Abs(size.x * size.y);
                int priority = GetMapPriority(candidate, fallback);
                if (priority > bestPriority || (priority == bestPriority && area < smallestArea))
                {
                    bestPriority = priority;
                    smallestArea = area;
                    bestMatch = candidate;
                }
            }

            return bestMatch ?? fallback;
        }

        private static int GetMapPriority(MapData candidate, MapData? activeMap)
        {
            if (!IsMainWorldMap(candidate))
            {
                return 30;
            }

            if (string.Equals(
                    candidate.Definition.name,
                    DetailedMainWorldDefinitionName,
                    StringComparison.OrdinalIgnoreCase))
            {
                return 20;
            }

            return candidate == activeMap ? 10 : 0;
        }

        private static bool IsMainWorldMap(MapData mapData)
        {
            string definitionName = mapData.Definition.name;
            return string.Equals(definitionName, MainWorldDefinitionName, StringComparison.OrdinalIgnoreCase)
                || string.Equals(definitionName, DetailedMainWorldDefinitionName, StringComparison.OrdinalIgnoreCase);
        }

        private void OnRegionChanged(Region region, bool hasEntered)
        {
            if (hasEntered)
            {
                _activeRegions.Remove(region);
                _activeRegions.Add(region);
                return;
            }

            _activeRegions.Remove(region);
        }

        private string GetRegionLabel()
        {
            for (int i = _activeRegions.Count - 1; i >= 0; i--)
            {
                Region region = _activeRegions[i];
                if (region.IsCaveRegion())
                {
                    return region.GetRegionName();
                }
            }

            for (int i = _activeRegions.Count - 1; i >= 0; i--)
            {
                Region region = _activeRegions[i];
                if (region != Region.Overworld_Or_Cave)
                {
                    return region.GetRegionName();
                }
            }

            return string.Empty;
        }

        private void HideView()
        {
            _view?.SetVisible(false);
        }

    }
}
