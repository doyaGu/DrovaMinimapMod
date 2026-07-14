using Drova_Modding_API.Access;
using Drova_Modding_API.UI.Builder;

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
        private MinimapPreferences _preferences = MinimapPreferences.Default;

        internal MinimapPreferences Current => _preferences;

        internal void LoadFromGameConfigIfAvailable()
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
            LoadFromGameConfigIfAvailable();
            OptionUIBuilder? builder = OptionMenuAccess.Instance.GetBuilder(MinimapLocalization.Namespace);
            if (builder == null)
            {
                return;
            }

            MinimapPreferences preferences = Current;
            builder
                .CreateTitle(MinimapLocalization.L("Title"))
                .CreateSwitch(MinimapLocalization.L("Enabled"), MinimapLocalization.L("On"), MinimapLocalization.L("Off"), EnabledKey, preferences.Enabled)
                .CreateSlider(MinimapLocalization.L("Size"), SizeKey, MinimapPreferences.MinimumSize, MinimapPreferences.MaximumSize, preferences.Size)
                .CreateSlider(MinimapLocalization.L("Zoom"), ZoomKey, MinimapPreferences.MinimumZoom, MinimapPreferences.MaximumZoom, preferences.Zoom, false)
                .CreateSlider(MinimapLocalization.L("Opacity"), OpacityKey, MinimapPreferences.MinimumOpacityPercentage, MinimapPreferences.MaximumOpacityPercentage, preferences.OpacityPercentage)
                .CreateSwitch(MinimapLocalization.L("ShowMarkers"), MinimapLocalization.L("On"), MinimapLocalization.L("Off"), ShowStandardMarkersKey, preferences.ShowStandardMarkers)
                .CreateSwitch(MinimapLocalization.L("ShowNpcMarkers"), MinimapLocalization.L("On"), MinimapLocalization.L("Off"), ShowNpcMarkersKey, preferences.ShowNpcMarkers)
                .Build();
        }

        internal void ReloadFromConfig()
        {
            MinimapPreferences current = Current;
            _preferences = new MinimapPreferences(
                ReadValue(EnabledKey, current.Enabled),
                ReadValue(SizeKey, current.Size),
                ReadValue(ZoomKey, current.Zoom),
                ReadValue(OpacityKey, current.OpacityPercentage),
                ReadValue(ShowStandardMarkersKey, current.ShowStandardMarkers),
                ReadValue(ShowNpcMarkersKey, current.ShowNpcMarkers));
        }

        private static T ReadValue<T>(string key, T fallback)
        {
            return ConfigAccessor.TryGetConfigValue<T>(key, out T value) ? value : fallback;
        }
    }
}
