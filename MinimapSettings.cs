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
        private const string UseAreaMapsKey = "DrovaMinimap_UseAreaMaps";

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
                .CreateSwitch(MinimapLocalization.L("UseAreaMaps"), MinimapLocalization.L("On"), MinimapLocalization.L("Off"), UseAreaMapsKey, preferences.UseAreaMaps)
                .CreateSlider(MinimapLocalization.L("Size"), SizeKey, MinimapPreferences.MinimumSize, MinimapPreferences.MaximumSize, preferences.Size)
                .CreateSlider(MinimapLocalization.L("Zoom"), ZoomKey, MinimapPreferences.MinimumZoom, MinimapPreferences.MaximumZoom, preferences.Zoom, false)
                .CreateSlider(MinimapLocalization.L("Opacity"), OpacityKey, MinimapPreferences.MinimumOpacityPercentage, MinimapPreferences.MaximumOpacityPercentage, preferences.OpacityPercentage)
                .Build();
        }

        internal void ReloadFromConfig()
        {
            MinimapPreferences current = Current;
            _preferences = new MinimapPreferences(
                enabled: ReadValue(EnabledKey, current.Enabled),
                size: ReadValue(SizeKey, current.Size),
                zoom: ReadValue(ZoomKey, current.Zoom),
                opacityPercentage: ReadValue(OpacityKey, current.OpacityPercentage),
                useAreaMaps: ReadValue(UseAreaMapsKey, current.UseAreaMaps));
        }

        private static T ReadValue<T>(string key, T fallback)
        {
            return ConfigAccessor.TryGetConfigValue<T>(key, out T value) ? value : fallback;
        }
    }
}
