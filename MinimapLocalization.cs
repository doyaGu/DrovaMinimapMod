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
            ["UseAreaMaps"] = "Switch to area maps",
            ["Title"] = "Minimap",
            ["Enabled"] = "Enable minimap",
            ["Size"] = "Size",
            ["Zoom"] = "Zoom",
            ["Opacity"] = "Opacity",
            ["On"] = "On",
            ["Off"] = "Off"
        };

        private static readonly IReadOnlyDictionary<string, string> German = new Dictionary<string, string>
        {
            ["UseAreaMaps"] = "Zu Gebietskarten wechseln",
            ["Title"] = "Minikarte",
            ["Enabled"] = "Minikarte aktivieren",
            ["Size"] = "Größe",
            ["Zoom"] = "Zoom",
            ["Opacity"] = "Deckkraft",
            ["On"] = "Ein",
            ["Off"] = "Aus"
        };

        private static readonly IReadOnlyDictionary<string, string> Spanish = new Dictionary<string, string>
        {
            ["UseAreaMaps"] = "Cambiar a mapas de zona",
            ["Title"] = "Minimapa",
            ["Enabled"] = "Activar minimapa",
            ["Size"] = "Tamaño",
            ["Zoom"] = "Zoom",
            ["Opacity"] = "Opacidad",
            ["On"] = "Activado",
            ["Off"] = "Desactivado"
        };

        private static readonly IReadOnlyDictionary<string, string> French = new Dictionary<string, string>
        {
            ["UseAreaMaps"] = "Basculer vers les cartes de zone",
            ["Title"] = "Mini-carte",
            ["Enabled"] = "Activer la mini-carte",
            ["Size"] = "Taille",
            ["Zoom"] = "Zoom",
            ["Opacity"] = "Opacité",
            ["On"] = "Activé",
            ["Off"] = "Désactivé"
        };

        private static readonly IReadOnlyDictionary<string, string> Korean = new Dictionary<string, string>
        {
            ["UseAreaMaps"] = "지역 지도로 전환",
            ["Title"] = "미니맵",
            ["Enabled"] = "미니맵 활성화",
            ["Size"] = "크기",
            ["Zoom"] = "확대",
            ["Opacity"] = "투명도",
            ["On"] = "켜기",
            ["Off"] = "끄기"
        };

        private static readonly IReadOnlyDictionary<string, string> Polish = new Dictionary<string, string>
        {
            ["UseAreaMaps"] = "Przełączaj na mapy obszarów",
            ["Title"] = "Minimapa",
            ["Enabled"] = "Włącz minimapę",
            ["Size"] = "Rozmiar",
            ["Zoom"] = "Powiększenie",
            ["Opacity"] = "Przezroczystość",
            ["On"] = "Włączone",
            ["Off"] = "Wyłączone"
        };

        private static readonly IReadOnlyDictionary<string, string> SimplifiedChinese = new Dictionary<string, string>
        {
            ["UseAreaMaps"] = "自动切换区域地图",
            ["Title"] = "小地图",
            ["Enabled"] = "启用小地图",
            ["Size"] = "尺寸",
            ["Zoom"] = "缩放",
            ["Opacity"] = "透明度",
            ["On"] = "开",
            ["Off"] = "关"
        };

        private static readonly IReadOnlyDictionary<string, string> TraditionalChinese = new Dictionary<string, string>
        {
            ["UseAreaMaps"] = "自動切換區域地圖",
            ["Title"] = "小地圖",
            ["Enabled"] = "啟用小地圖",
            ["Size"] = "尺寸",
            ["Zoom"] = "縮放",
            ["Opacity"] = "透明度",
            ["On"] = "開",
            ["Off"] = "關"
        };

        // Keep this aligned with Drova's shipped Localization directories. The
        // game exposes additional ELanguage values, but without a language
        // directory they cannot be selected or loaded by Drova.
        private static readonly ELanguage[] SupportedLanguages =
        [
            ELanguage.de,
            ELanguage.en,
            ELanguage.es,
            ELanguage.fr,
            ELanguage.ko,
            ELanguage.pl,
            ELanguage.zh_CN,
            ELanguage.zh_TW
        ];

        private static bool _registered;

        internal static void Register()
        {
            if (_registered)
            {
                return;
            }

            List<LocalizationAccess.LocalizationEntry> entries = [];
            foreach (ELanguage language in SupportedLanguages)
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
                "de" => German,
                "es" => Spanish,
                "fr" => French,
                "ko" => Korean,
                "pl" => Polish,
                "zh_CN" => SimplifiedChinese,
                "zh_TW" => TraditionalChinese,
                _ => English
            };
        }
    }
}
