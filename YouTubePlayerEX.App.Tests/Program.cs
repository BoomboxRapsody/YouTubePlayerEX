// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework;
using osu.Framework.Platform;

namespace YouTubePlayerEX.App.Tests
{
    public static class Program
    {
        public static void Main()
        {
            using (GameHost host = Host.GetSuitableDesktopHost("YouTubePlayerEX-VisualTests"))
            using (var game = new YouTubePlayerEXTestBrowser())
                host.Run(game);
        }
    }
}
