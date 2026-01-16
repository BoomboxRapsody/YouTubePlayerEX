using System;
using System.Threading.Tasks;
using YouTubePlayerEX.App;
using YouTubePlayerEX.App.Extensions;
using YouTubePlayerEX.App.Updater;
using YouTubePlayerEX.Desktop.Updater;

namespace YouTubePlayerEX.Desktop
{
    internal partial class YouTubePlayerEXAppDesktop : YouTubePlayerEXApp
    {
        protected override UpdateManager CreateUpdateManager() => new VelopackUpdateManager();

        public override bool RestartAppWhenExited()
        {
            Task.Run(() => Velopack.UpdateExe.Start(waitPid: (uint)Environment.ProcessId)).FireAndForget();
            return true;
        }
    }
}
