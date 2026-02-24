// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using YouTubePlayerEX.App;
using YouTubePlayerEX.App.Extensions;
using YouTubePlayerEX.App.Updater;
using YouTubePlayerEX.Linux.Updater;

namespace YouTubePlayerEX.Linux
{
    internal partial class YouTubePlayerEXAppLinuxDesktop : YouTubePlayerEXApp
    {
        protected override UpdateManager CreateUpdateManager() => new VelopackUpdateManager();

        public override bool RestartAppWhenExited()
        {
            Task.Run(() => Velopack.UpdateExe.Start(waitPid: (uint)Environment.ProcessId)).FireAndForget();
            return true;
        }
    }
}
