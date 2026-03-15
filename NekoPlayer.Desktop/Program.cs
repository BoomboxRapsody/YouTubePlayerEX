// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework;
using osu.Framework.Development;
using osu.Framework.Platform;
using Velopack;

namespace NekoPlayer.Desktop
{
    public static class Program
    {
        public static void Main()
        {
            string gameName = @"NekoPlayer";

            VelopackApp.Build().Run();

            HostOptions hostOptions = new HostOptions
            {
                FriendlyGameName = "NekoPlayer",
            };

            if (DebugUtils.IsDebugBuild)
                gameName = "NekoPlayer-development";

            using (GameHost host = Host.GetSuitableDesktopHost(gameName, hostOptions))
            {
                host.AllowBenchmarkUnlimitedFrames = true;
                host.Run(new NekoPlayerAppDesktop());
            }
        }
    }
}
