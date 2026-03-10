// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework;
using osu.Framework.Platform;
using Velopack;

namespace NekoPlayer.Desktop
{
    public static class Program
    {
        public static void Main()
        {
            VelopackApp.Build().Run();

            HostOptions hostOptions = new HostOptions
            {
                FriendlyGameName = "NekoPlayer",
            };

            using (GameHost host = Host.GetSuitableDesktopHost(@"NekoPlayer", hostOptions))
            {
                host.AllowBenchmarkUnlimitedFrames = true;
                host.Run(new NekoPlayerAppDesktop());
            }
        }
    }
}
