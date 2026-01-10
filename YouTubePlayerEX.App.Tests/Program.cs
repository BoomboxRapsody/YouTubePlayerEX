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
