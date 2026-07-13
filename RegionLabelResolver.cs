using Drova_Modding_API.Systems;
using Il2CppCustomFramework.Localization;

namespace DrovaMinimapMod
{
    /// <summary>
    /// Resolves AreaNameSystem regions through Drova's native AreaNames
    /// localization path. The API region enum intentionally abstracts a few
    /// scene keys, so those aliases are kept at this single compatibility seam.
    /// </summary>
    internal static class RegionLabelResolver
    {
        internal static string Resolve(Region region)
        {
            string? areaKey = GetAreaKey(region);
            if (areaKey == null)
            {
                return string.Empty;
            }

            LocalizationDB? localization = LocalizationDB.Instance;
            if (localization == null)
            {
                return region.GetRegionName();
            }

            string label = localization.LocalizeArea(areaKey, null);
            return string.IsNullOrWhiteSpace(label) ? region.GetRegionName() : label;
        }

        private static string? GetAreaKey(Region region)
        {
            return region switch
            {
                Region.RedTower => "RedTower",
                Region.Mine => "Mine",
                Region.Cave => "Cave",
                // Drova's city region is named Nemeton in AreaNames.loc.
                Region.City => "Nemeton",
                Region.SpiderDungeon => "SpiderDungeon",
                Region.Auwald => "Auwald",
                Region.Nemeton => "Nemeton",
                Region.EntryNemeton => "EntryNemeton",
                // AreaNameSystem combines the two intro scene keys.
                Region.Intro => "Intro_Real",
                Region.Ruins => "Ruins",
                Region.Tavern => "Tavern",
                Region.CityDungeon => "CityDungeon",
                Region.DeathMoor => "DeathMoor",
                Region.Academy => "Academy",
                Region.Forest => "Forest",
                Region.Library => "Library",
                Region.FriendlyMoor => "FriendlyMoor",
                Region.Mutter => "Mutter",
                Region.Leuchtwald => "Leuchtwald",
                Region.River => "River",
                Region.RootenMoor => "RottenMoor",
                Region.WoodCamp => "Woodcamp",
                Region.RuinsCamp => "Ruincamp",
                Region.RuinUnder => "RuinUnder",
                Region.Magecamp => "Magiecamp",
                Region.Ruinexplorer => "RuinExplorer",
                Region.RuinSchmuggler => "RuinSchmuggler",
                Region.Hain => "NemetonHain",
                Region.Heide => "Heide",
                Region.Schlund => "Schlund",
                _ => null
            };
        }
    }
}
