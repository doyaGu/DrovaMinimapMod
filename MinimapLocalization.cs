using Drova_Modding_API.Access;
using Il2Cpp;
using Il2CppCustomFramework.Localization;
using static Il2CppCustomFramework.Localization.LocalizationDB;

namespace DrovaMinimapMod
{
    /// <summary>
    /// Registers all shipped translations before any localized UI is built.
    /// LocalizedString references then resolve dynamically when Drova changes language.
    /// </summary>
    internal static class MinimapLocalization
    {
        internal const string Namespace = "DrovaMinimap";

        private static readonly IReadOnlyDictionary<string, string> English = new Dictionary<string, string>
        {
            ["Title"] = "Minimap",
            ["Enabled"] = "Enable minimap",
            ["Size"] = "Size",
            ["Zoom"] = "Zoom",
            ["Opacity"] = "Opacity",
            ["ShowMarkers"] = "Show map markers",
            ["ShowNpcMarkers"] = "Show NPC markers",
            ["On"] = "On",
            ["Off"] = "Off"
        };

        private static readonly IReadOnlyDictionary<string, string> SimplifiedChinese = new Dictionary<string, string>
        {
            ["Title"] = "小地图",
            ["Enabled"] = "启用小地图",
            ["Size"] = "尺寸",
            ["Zoom"] = "缩放",
            ["Opacity"] = "透明度",
            ["ShowMarkers"] = "显示普通标记",
            ["ShowNpcMarkers"] = "显示 NPC 标记",
            ["On"] = "开",
            ["Off"] = "关"
        };

        private static readonly IReadOnlyDictionary<string, string> TraditionalChinese = new Dictionary<string, string>
        {
            ["Title"] = "小地圖",
            ["Enabled"] = "啟用小地圖",
            ["Size"] = "尺寸",
            ["Zoom"] = "縮放",
            ["Opacity"] = "透明度",
            ["ShowMarkers"] = "顯示一般標記",
            ["ShowNpcMarkers"] = "顯示 NPC 標記",
            ["On"] = "開",
            ["Off"] = "關"
        };

        private static bool _registered;

        internal static void Register()
        {
            if (_registered)
            {
                return;
            }

            List<LocalizationAccess.LocalizationEntry> entries = [];
            foreach (ELanguage language in Enum.GetValues<ELanguage>())
            {
                AddEntries(entries, language, GetTranslations(language));
            }

            LocalizationAccess.CreateLocalizationEntries(entries, Namespace);
            _registered = true;
        }

        internal static LocalizedString L(string key)
        {
            return LocalizationAccess.GetLocalizedString(Namespace, key);
        }

        private static void AddEntries(
            List<LocalizationAccess.LocalizationEntry> entries,
            ELanguage language,
            IReadOnlyDictionary<string, string> translations)
        {
            foreach (KeyValuePair<string, string> translation in translations)
            {
                entries.Add(new LocalizationAccess.LocalizationEntry(translation.Key, translation.Value, language));
            }
        }

        private static IReadOnlyDictionary<string, string> GetTranslations(ELanguage language)
        {
            return language.ToString() switch
            {
                "zh_CN" => SimplifiedChinese,
                "zh_TW" => TraditionalChinese,
                _ => English
            };
        }
    }
}
