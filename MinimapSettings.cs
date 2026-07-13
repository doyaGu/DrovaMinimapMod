using Drova_Modding_API.Access;
using Drova_Modding_API.UI.Builder;
using Il2Cpp;
using Il2CppCustomFramework.Localization;
using static Il2CppCustomFramework.Localization.LocalizationDB;
using UnityEngine;

namespace DrovaMinimapMod
{
    internal sealed class MinimapSettings
    {
        private const string LocalizationNamespace = "DrovaMinimap";
        private const string EnabledKey = "DrovaMinimap_Enabled";
        private const string SizeKey = "DrovaMinimap_Size";
        private const string ZoomKey = "DrovaMinimap_Zoom";
        private const string OpacityKey = "DrovaMinimap_Opacity";
        // Keep the established key so existing player configuration continues to work.
        private const string ShowStandardMarkersKey = "DrovaMinimap_ShowPlayerMarkers";
        private const string ShowNpcMarkersKey = "DrovaMinimap_ShowNpcMarkers";

        private static readonly HashSet<ELanguage> RegisteredLocalizationLanguages = [];
        private bool _gameConfigLoaded;

        public bool Enabled { get; private set; } = true;
        public int Size { get; private set; } = 240;
        public float Zoom { get; private set; } = 2f;
        public float Opacity { get; private set; } = 0.85f;
        public bool ShowStandardMarkers { get; private set; } = true;
        public bool ShowNpcMarkers { get; private set; } = true;

        public void TryLoadFromGameConfig()
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

        public void BuildOptions()
        {
            EnsureLocalization();

            OptionUIBuilder? builder = OptionMenuAccess.Instance.GetBuilder(LocalizationNamespace);
            if (builder == null)
            {
                return;
            }

            builder
                .CreateTitle(L("Title"))
                .CreateSwitch(L("Enabled"), L("On"), L("Off"), EnabledKey, Enabled)
                .CreateSlider(L("Size"), SizeKey, 160, 420, Size)
                .CreateSlider(L("Zoom"), ZoomKey, 1f, 4f, Zoom, false)
                .CreateSlider(L("Opacity"), OpacityKey, 40, 100, Mathf.RoundToInt(Opacity * 100f))
                .CreateSwitch(L("ShowPlayerMarkers"), L("On"), L("Off"), ShowStandardMarkersKey, ShowStandardMarkers)
                .CreateSwitch(L("ShowNpcMarkers"), L("On"), L("Off"), ShowNpcMarkersKey, ShowNpcMarkers)
                .Build();
        }

        public void ReloadFromConfig()
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

        private static LocalizedString L(string key)
        {
            return LocalizationAccess.GetLocalizedString(LocalizationNamespace, key);
        }

        private static void EnsureLocalization()
        {
            ELanguage currentLanguage = LocalizationDB.Instance.CurrentLanguage;
            if (RegisteredLocalizationLanguages.Contains(currentLanguage))
            {
                return;
            }

            Dictionary<string, string> english = new()
            {
                ["Title"] = "Minimap",
                ["Enabled"] = "Enable minimap",
                ["Size"] = "Size",
                ["Zoom"] = "Zoom",
                ["Opacity"] = "Opacity",
                ["ShowPlayerMarkers"] = "Show player markers",
                ["ShowNpcMarkers"] = "Show NPC markers",
                ["On"] = "On",
                ["Off"] = "Off"
            };
            Dictionary<string, string> chinese = new()
            {
                ["Title"] = "小地图",
                ["Enabled"] = "启用小地图",
                ["Size"] = "尺寸",
                ["Zoom"] = "缩放",
                ["Opacity"] = "透明度",
                ["ShowPlayerMarkers"] = "显示普通标记",
                ["ShowNpcMarkers"] = "显示 NPC 标记",
                ["On"] = "开",
                ["Off"] = "关"
            };
            Dictionary<string, string> source = currentLanguage.ToString().StartsWith("zh_", StringComparison.OrdinalIgnoreCase)
                ? chinese
                : english;
            List<LocalizationAccess.LocalizationEntry> entries = [];
            foreach (KeyValuePair<string, string> pair in source)
            {
                entries.Add(new LocalizationAccess.LocalizationEntry(pair.Key, pair.Value, currentLanguage));
            }

            LocalizationAccess.CreateLocalizationEntries(entries, LocalizationNamespace);
            RegisteredLocalizationLanguages.Add(currentLanguage);
        }
    }
}
