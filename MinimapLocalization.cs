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

        private static readonly IReadOnlyDictionary<string, string> German = new Dictionary<string, string>
        {
            ["Title"] = "Minikarte",
            ["Enabled"] = "Minikarte aktivieren",
            ["Size"] = "Größe",
            ["Zoom"] = "Zoom",
            ["Opacity"] = "Deckkraft",
            ["ShowMarkers"] = "Kartenmarkierungen anzeigen",
            ["ShowNpcMarkers"] = "NPC-Markierungen anzeigen",
            ["On"] = "Ein",
            ["Off"] = "Aus"
        };

        private static readonly IReadOnlyDictionary<string, string> Spanish = new Dictionary<string, string>
        {
            ["Title"] = "Minimapa",
            ["Enabled"] = "Activar minimapa",
            ["Size"] = "Tamaño",
            ["Zoom"] = "Zoom",
            ["Opacity"] = "Opacidad",
            ["ShowMarkers"] = "Mostrar marcadores del mapa",
            ["ShowNpcMarkers"] = "Mostrar marcadores de PNJ",
            ["On"] = "Activado",
            ["Off"] = "Desactivado"
        };

        private static readonly IReadOnlyDictionary<string, string> French = new Dictionary<string, string>
        {
            ["Title"] = "Mini-carte",
            ["Enabled"] = "Activer la mini-carte",
            ["Size"] = "Taille",
            ["Zoom"] = "Zoom",
            ["Opacity"] = "Opacité",
            ["ShowMarkers"] = "Afficher les marqueurs de carte",
            ["ShowNpcMarkers"] = "Afficher les marqueurs de PNJ",
            ["On"] = "Activé",
            ["Off"] = "Désactivé"
        };

        private static readonly IReadOnlyDictionary<string, string> Korean = new Dictionary<string, string>
        {
            ["Title"] = "미니맵",
            ["Enabled"] = "미니맵 활성화",
            ["Size"] = "크기",
            ["Zoom"] = "확대",
            ["Opacity"] = "투명도",
            ["ShowMarkers"] = "지도 표식 표시",
            ["ShowNpcMarkers"] = "NPC 표식 표시",
            ["On"] = "켜기",
            ["Off"] = "끄기"
        };

        private static readonly IReadOnlyDictionary<string, string> Polish = new Dictionary<string, string>
        {
            ["Title"] = "Minimapa",
            ["Enabled"] = "Włącz minimapę",
            ["Size"] = "Rozmiar",
            ["Zoom"] = "Powiększenie",
            ["Opacity"] = "Przezroczystość",
            ["ShowMarkers"] = "Pokaż znaczniki mapy",
            ["ShowNpcMarkers"] = "Pokaż znaczniki NPC",
            ["On"] = "Włączone",
            ["Off"] = "Wyłączone"
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
