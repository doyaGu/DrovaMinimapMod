using Drova_Modding_API.Access;
using Drova_Modding_API.GlobalFields;
using Il2CppDrova;
using MelonLoader;

[assembly: MelonInfo(typeof(DrovaMinimapMod.Core), "Drova Minimap", DrovaMinimapMod.BuildVersion.Value, "kakut")]
[assembly: MelonGame("Just2D", "Drova")]
[assembly: MelonAdditionalDependencies("Drova_Modding_API")]

namespace DrovaMinimapMod
{
    public sealed class Core : MelonMod
    {
        private readonly MinimapSettings _settings = new();
        private readonly MinimapController _controller = new();

        public override void OnInitializeMelon()
        {
            _controller.Initialize();
            PlayerAccess.OnPlayerFound += OnPlayerFound;
            OptionMenuAccess.Instance.OnOptionMenuOpen += BuildOptions;
            OptionMenuAccess.Instance.OnOptionMenuClose += ReloadSettings;
            LoggerInstance.Msg("Drova Minimap initialized.");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            _controller.OnSceneLoaded();

            if (sceneName == SceneNames.MainMenu)
            {
                // Drova's LocalizationDB is created with the main-menu scene. The
                // Modding API registers its own localization entries at this point.
                MinimapLocalization.Register();
            }
        }

        public override void OnUpdate()
        {
            _controller.Tick(_settings.Current);
        }

        public override void OnDeinitializeMelon()
        {
            PlayerAccess.OnPlayerFound -= OnPlayerFound;
            OptionMenuAccess.Instance.OnOptionMenuOpen -= BuildOptions;
            OptionMenuAccess.Instance.OnOptionMenuClose -= ReloadSettings;
            _controller.Dispose();
        }

        private void BuildOptions()
        {
            _settings.BuildOptions();
        }

        private void OnPlayerFound(Actor player)
        {
            _settings.LoadFromGameConfigIfAvailable();
            _controller.OnPlayerFound(player);
        }

        private void ReloadSettings()
        {
            _settings.ReloadFromConfig();
        }
    }
}
