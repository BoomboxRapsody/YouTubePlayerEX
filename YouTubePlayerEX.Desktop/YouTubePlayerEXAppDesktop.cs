using YouTubePlayerEX.App;
using YouTubePlayerEX.App.Updater;
using YouTubePlayerEX.Desktop.Updater;

namespace YouTubePlayerEX.Desktop
{
    internal partial class YouTubePlayerEXAppDesktop : YouTubePlayerEXApp
    {
        protected override UpdateManager CreateUpdateManager() => new VelopackUpdateManager();
    }
}
