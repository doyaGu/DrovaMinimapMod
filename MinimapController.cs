using Drova_Modding_API.Access;
using Drova_Modding_API.Systems;
using Il2CppDrova;
using UnityEngine;

namespace DrovaMinimapMod
{
    internal sealed class MinimapController
    {
        private readonly List<Region> _activeRegions = [];
        private readonly MainWorldMapResolver _mapResolver = new();
        private readonly NativeMapPresentation _nativeMapPresentation = new();
        private Actor? _player;
        private AreaNameSystem? _areaNameSystem;
        private MinimapView? _view;

        internal void OnPlayerFound(Actor player)
        {
            _player = player;
            _activeRegions.Clear();
            _nativeMapPresentation.Reset();
            _view?.Dispose();
            _view = null;
            RefreshAreaSubscription();
        }

        internal void RefreshAreaSubscription()
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

        internal void Tick(MinimapPreferences preferences)
        {
            RefreshAreaSubscription();

            if (_player == null || !_player)
            {
                HideView();
                return;
            }

            if (!preferences.Enabled)
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

            bool guiSuppressed = guiHandler.GUIIsHidden
                                 || guiHandler.HUDIsHidden
                                 || guiHandler.IsPlayerGameMenuWindowVisible()
                                 || guiHandler.HasOpenModalWindows();
            if (guiSuppressed)
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
            var mapData = _mapResolver.TryResolve(playerWorldPosition);
            if (mapData == null)
            {
                _view.SetVisible(false);
                return;
            }

            Vector2 lookDirection = _player.GetLookModule()?.CurrentLookDir ?? Vector2.up;
            var mapPresentation = _nativeMapPresentation.Refresh(mapData, playerWorldPosition);
            _view.Render(new MinimapFrame(
                mapData,
                mapPresentation,
                lookDirection,
                GetRegionLabel(),
                preferences));
            _view.SetVisible(true);
        }

        internal void Dispose()
        {
            if (_areaNameSystem != null)
            {
                _areaNameSystem.OnRegionChanged -= OnRegionChanged;
                _areaNameSystem = null;
            }

            _view?.Dispose();
            _view = null;
            _nativeMapPresentation.Reset();
            MinimapView.ReleaseSharedResources();
            _player = null;
            _activeRegions.Clear();
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
                    return RegionLabelResolver.Resolve(region);
                }
            }

            for (int i = _activeRegions.Count - 1; i >= 0; i--)
            {
                Region region = _activeRegions[i];
                if (region != Region.Overworld_Or_Cave)
                {
                    return RegionLabelResolver.Resolve(region);
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
