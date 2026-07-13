using Drova_Modding_API.Access;
using Drova_Modding_API.GlobalFields;
using MelonLoader;

[assembly: MelonInfo(typeof(DrovaMinimapMod.Core), "Drova Minimap", "1.0.0", "kakut")]
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
            PlayerAccess.OnPlayerFound += _controller.OnPlayerFound;
            OptionMenuAccess.Instance.OnOptionMenuOpen += BuildOptions;
            OptionMenuAccess.Instance.OnOptionMenuClose += ReloadSettings;
            LoggerInstance.Msg("Drova Minimap initialized.");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            if (sceneName == SceneNames.MainMenu)
            {
                // Drova's LocalizationDB is created with the main-menu scene. The
                // Modding API registers its own localization entries at this point.
                MinimapLocalization.Register();
            }

            _controller.RefreshAreaSubscription();
        }

        public override void OnUpdate()
        {
            _settings.TryLoadFromGameConfig();
            _controller.Tick(_settings);
        }

        public override void OnDeinitializeMelon()
        {
            PlayerAccess.OnPlayerFound -= _controller.OnPlayerFound;
            OptionMenuAccess.Instance.OnOptionMenuOpen -= BuildOptions;
            OptionMenuAccess.Instance.OnOptionMenuClose -= ReloadSettings;
            _controller.Dispose();
            MinimapView.ReleaseSharedResources();
            MarkerRenderer.ReleaseSharedResources();
        }

        private void BuildOptions()
        {
            _settings.BuildOptions();
        }

        private void ReloadSettings()
        {
            _settings.ReloadFromConfig();
        }
    }
}
