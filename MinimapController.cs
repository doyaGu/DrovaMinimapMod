using Drova_Modding_API.Access;
using Drova_Modding_API.Systems;
using Il2CppDrova;
using UnityEngine;

namespace DrovaMinimapMod
{
    internal sealed class MinimapController
    {
        private readonly List<Region> _activeRegions = [];
        private readonly PlayerPositionMapResolver _mapResolver = new();
        private readonly NativeMapPresentation _nativeMapPresentation = new();
        private Actor? _player;
        private AreaNameSystem? _areaNameSystem;
        private MinimapView? _view;
        private bool _mapPresentationSuspended;
        private bool _areaLifecycleSubscribed;

        internal void Initialize()
        {
            if (_areaLifecycleSubscribed)
            {
                return;
            }

            AreaNameSystem.OnInstanceChanged += OnAreaNameSystemChanged;
            _areaLifecycleSubscribed = true;
            AttachAreaNameSystem(AreaNameSystem.Instance);
        }

        internal void OnPlayerFound(Actor player)
        {
            _player = player;
            _activeRegions.Clear();
            _nativeMapPresentation.Reset();
            _mapPresentationSuspended = false;
            _view?.Dispose();
            _view = null;
            RebuildAreaState();
        }

        internal void OnSceneLoaded()
        {
            _nativeMapPresentation.Reset();
            _mapPresentationSuspended = false;
            _view?.Dispose();
            _view = null;
        }

        private void OnAreaNameSystemChanged(AreaNameSystem? areaNameSystem)
        {
            AttachAreaNameSystem(areaNameSystem);
        }

        private void AttachAreaNameSystem(AreaNameSystem? areaNameSystem)
        {
            if (ReferenceEquals(_areaNameSystem, areaNameSystem))
            {
                return;
            }

            if (_areaNameSystem != null)
            {
                _areaNameSystem.OnRegionChanged -= OnRegionChanged;
            }

            _areaNameSystem = areaNameSystem;
            if (_areaNameSystem != null)
            {
                _areaNameSystem.OnRegionChanged += OnRegionChanged;
            }

            RebuildAreaState();
        }

        internal void Tick(MinimapPreferences preferences)
        {
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

            // Match GUI_MapPlayerMarker: the native full-map marker is placed
            // from the entity's feet position, not the actor transform pivot.
            Vector2 playerWorldPosition = _player.GetFeetPosition();
            var mapData = _mapResolver.TryResolve(playerWorldPosition, preferences.UseAreaMaps);
            if (mapData == null)
            {
                SuspendMapPresentation();
                return;
            }

            _mapPresentationSuspended = false;
            Vector2 lookDirection = _player.GetLookModule()?.CurrentLookDir ?? Vector2.up;
            var mapPresentation = _nativeMapPresentation.Resolve(mapData, playerWorldPosition);
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
            if (_areaLifecycleSubscribed)
            {
                AreaNameSystem.OnInstanceChanged -= OnAreaNameSystemChanged;
                _areaLifecycleSubscribed = false;
            }

            if (_areaNameSystem != null)
            {
                _areaNameSystem.OnRegionChanged -= OnRegionChanged;
                _areaNameSystem = null;
            }

            _view?.Dispose();
            _view = null;
            _nativeMapPresentation.Reset();
            _mapPresentationSuspended = false;
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
            }
            else
            {
                _activeRegions.Remove(region);
            }

        }

        private void RebuildAreaState()
        {
            _activeRegions.Clear();
            if (_areaNameSystem == null)
            {
                return;
            }

            foreach (Region region in _areaNameSystem.GetCurrentRegions())
            {
                _activeRegions.Remove(region);
                _activeRegions.Add(region);
            }

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

        private void SuspendMapPresentation()
        {
            if (!_mapPresentationSuspended)
            {
                _nativeMapPresentation.Reset();
                _mapPresentationSuspended = true;
            }

            HideView();
        }

    }
}
