// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Platform;
using osu.Framework;
using Velopack;

namespace YouTubePlayerEX.Windows
{
    public static class Program
    {
        public static void Main()
        {
            VelopackApp.Build().Run();

            HostOptions hostOptions = new HostOptions
            {
                FriendlyGameName = "YouTube Player EX",
            };

            using (GameHost host = Host.GetSuitableDesktopHost(@"YouTubePlayerEX", hostOptions))
            {
                host.AllowBenchmarkUnlimitedFrames = true;
                host.Run(new YouTubePlayerEXAppWindowsDesktop());
            }
        }
    }
}
