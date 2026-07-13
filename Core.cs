using Drova_Modding_API.Access;
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
            _settings.LoadPersistentValues();
            PlayerAccess.OnPlayerFound += _controller.OnPlayerFound;
            OptionMenuAccess.Instance.OnOptionMenuOpen += BuildOptions;
            OptionMenuAccess.Instance.OnOptionMenuClose += ReloadSettings;
            LoggerInstance.Msg("Drova Minimap initialized.");
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            _controller.RefreshAreaSubscription();
        }

        public override void OnUpdate()
        {
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
