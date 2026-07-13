using Drova_Modding_API.Access;
using Drova_Modding_API.UI.Builder;
using UnityEngine;

namespace DrovaMinimapMod
{
    internal sealed class MinimapSettings
    {
        private const string EnabledKey = "DrovaMinimap_Enabled";
        private const string SizeKey = "DrovaMinimap_Size";
        private const string ZoomKey = "DrovaMinimap_Zoom";
        private const string OpacityKey = "DrovaMinimap_Opacity";

        // Preserve the existing config key so player settings remain compatible.
        private const string ShowStandardMarkersKey = "DrovaMinimap_ShowPlayerMarkers";
        private const string ShowNpcMarkersKey = "DrovaMinimap_ShowNpcMarkers";
        private bool _gameConfigLoaded;

        internal bool Enabled { get; private set; } = true;
        internal int Size { get; private set; } = MinimapPreferences.DefaultSize;
        internal float Zoom { get; private set; } = MinimapPreferences.DefaultZoom;
        internal float Opacity { get; private set; } = MinimapPreferences.DefaultOpacity;
        internal bool ShowStandardMarkers { get; private set; } = true;
        internal bool ShowNpcMarkers { get; private set; } = true;

        internal MinimapPreferences Current => new(
            Enabled,
            Size,
            Zoom,
            Opacity,
            ShowStandardMarkers,
            ShowNpcMarkers);

        internal void TryLoadFromGameConfig()
        {
            if (_gameConfigLoaded || ProviderAccess.GetDrovaResourceProvider() == null)
            {
                return;
            }

            var configHandler = ProviderAccess.GetConfigGameHandler();
            if (configHandler == null
                || configHandler.GameplayConfig == null
                || configHandler.GameplayConfig.ConfigFile == null)
            {
                return;
            }

            ReloadFromConfig();
            _gameConfigLoaded = true;
        }

        internal void BuildOptions()
        {
            TryLoadFromGameConfig();
            OptionUIBuilder? builder = OptionMenuAccess.Instance.GetBuilder(MinimapLocalization.Namespace);
            if (builder == null)
            {
                return;
            }

            builder
                .CreateTitle(MinimapLocalization.L("Title"))
                .CreateSwitch(MinimapLocalization.L("Enabled"), MinimapLocalization.L("On"), MinimapLocalization.L("Off"), EnabledKey, Enabled)
                .CreateSlider(MinimapLocalization.L("Size"), SizeKey, 160, 420, Size)
                .CreateSlider(MinimapLocalization.L("Zoom"), ZoomKey, 1f, 4f, Zoom, false)
                .CreateSlider(MinimapLocalization.L("Opacity"), OpacityKey, 40, 100, Mathf.RoundToInt(Opacity * 100f))
                .CreateSwitch(MinimapLocalization.L("ShowMarkers"), MinimapLocalization.L("On"), MinimapLocalization.L("Off"), ShowStandardMarkersKey, ShowStandardMarkers)
                .CreateSwitch(MinimapLocalization.L("ShowNpcMarkers"), MinimapLocalization.L("On"), MinimapLocalization.L("Off"), ShowNpcMarkersKey, ShowNpcMarkers)
                .Build();

        }

        internal void ReloadFromConfig()
        {
            if (ConfigAccessor.TryGetConfigValue<bool>(EnabledKey, out bool enabled))
            {
                Enabled = enabled;
            }

            if (ConfigAccessor.TryGetConfigValue<int>(SizeKey, out int size))
            {
                Size = Mathf.Clamp(size, 160, 420);
            }

            if (ConfigAccessor.TryGetConfigValue<float>(ZoomKey, out float zoom))
            {
                Zoom = Mathf.Clamp(zoom, 1f, 4f);
            }

            if (ConfigAccessor.TryGetConfigValue<int>(OpacityKey, out int opacity))
            {
                Opacity = Mathf.Clamp(opacity, 40, 100) / 100f;
            }

            if (ConfigAccessor.TryGetConfigValue<bool>(ShowStandardMarkersKey, out bool showStandardMarkers))
            {
                ShowStandardMarkers = showStandardMarkers;
            }

            if (ConfigAccessor.TryGetConfigValue<bool>(ShowNpcMarkersKey, out bool showNpcMarkers))
            {
                ShowNpcMarkers = showNpcMarkers;
            }
        }

    }
}
