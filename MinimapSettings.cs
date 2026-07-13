using Drova_Modding_API.Access;
using Drova_Modding_API.UI.Builder;
using Il2Cpp;
using Il2CppCustomFramework.Localization;
using MelonLoader;
using System.Text.Json;
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
        private const string ShowPlayerMarkersKey = "DrovaMinimap_ShowPlayerMarkers";
        private const string ShowNpcMarkersKey = "DrovaMinimap_ShowNpcMarkers";

        private static readonly HashSet<ELanguage> RegisteredLocalizationLanguages = [];
        private static readonly string StoragePath = Path.Combine(
            AppContext.BaseDirectory,
            "UserData",
            "DrovaMinimapMod.json");

        private bool _persistentValuesLoaded;

        public bool Enabled { get; private set; } = true;
        public int Size { get; private set; } = 240;
        public float Zoom { get; private set; } = 2f;
        public float Opacity { get; private set; } = 0.85f;
        public bool ShowPlayerMarkers { get; private set; } = true;
        public bool ShowNpcMarkers { get; private set; } = true;

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
                .CreateSwitch(L("ShowPlayerMarkers"), L("On"), L("Off"), ShowPlayerMarkersKey, ShowPlayerMarkers)
                .CreateSwitch(L("ShowNpcMarkers"), L("On"), L("Off"), ShowNpcMarkersKey, ShowNpcMarkers)
                .Build();
        }

        public void ReloadFromConfig()
        {
            bool changed = false;
            if (ConfigAccessor.TryGetConfigValue<bool>(EnabledKey, out bool enabled))
            {
                Enabled = enabled;
                changed = true;
            }

            if (ConfigAccessor.TryGetConfigValue<int>(SizeKey, out int size))
            {
                Size = Mathf.Clamp(size, 160, 420);
                changed = true;
            }

            if (ConfigAccessor.TryGetConfigValue<float>(ZoomKey, out float zoom))
            {
                Zoom = Mathf.Clamp(zoom, 1f, 4f);
                changed = true;
            }

            if (ConfigAccessor.TryGetConfigValue<int>(OpacityKey, out int opacity))
            {
                Opacity = Mathf.Clamp(opacity, 40, 100) / 100f;
                changed = true;
            }

            if (ConfigAccessor.TryGetConfigValue<bool>(ShowPlayerMarkersKey, out bool showPlayerMarkers))
            {
                ShowPlayerMarkers = showPlayerMarkers;
                changed = true;
            }

            if (ConfigAccessor.TryGetConfigValue<bool>(ShowNpcMarkersKey, out bool showNpcMarkers))
            {
                ShowNpcMarkers = showNpcMarkers;
                changed = true;
            }

            if (changed)
            {
                SavePersistentValues();
            }
        }

        public void LoadPersistentValues()
        {
            if (_persistentValuesLoaded)
            {
                return;
            }

            _persistentValuesLoaded = true;
            try
            {
                if (!File.Exists(StoragePath))
                {
                    return;
                }

                StoredValues? values = JsonSerializer.Deserialize<StoredValues>(File.ReadAllText(StoragePath));
                if (values == null)
                {
                    return;
                }

                Enabled = values.Enabled;
                Size = Mathf.Clamp(values.Size, 160, 420);
                Zoom = Mathf.Clamp(values.Zoom, 1f, 4f);
                Opacity = Mathf.Clamp(values.Opacity, 0.4f, 1f);
                ShowPlayerMarkers = values.ShowPlayerMarkers;
                ShowNpcMarkers = values.ShowNpcMarkers;
            }
            catch (Exception exception)
            {
                MelonLogger.Warning($"Unable to load minimap settings: {exception.Message}");
            }
        }

        private void SavePersistentValues()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(StoragePath)!);
                StoredValues values = new()
                {
                    Enabled = Enabled,
                    Size = Size,
                    Zoom = Zoom,
                    Opacity = Opacity,
                    ShowPlayerMarkers = ShowPlayerMarkers,
                    ShowNpcMarkers = ShowNpcMarkers
                };
                File.WriteAllText(StoragePath, JsonSerializer.Serialize(values, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception exception)
            {
                MelonLogger.Warning($"Unable to save minimap settings: {exception.Message}");
            }
        }

        private sealed class StoredValues
        {
            public bool Enabled { get; set; } = true;
            public int Size { get; set; } = 240;
            public float Zoom { get; set; } = 2f;
            public float Opacity { get; set; } = 0.85f;
            public bool ShowPlayerMarkers { get; set; } = true;
            public bool ShowNpcMarkers { get; set; } = true;
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
