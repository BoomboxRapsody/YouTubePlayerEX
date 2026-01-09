using osu.Framework;
using osu.Framework.Platform;

namespace YouTubePlayerEX.App.Tests
{
    public static class Program
    {
        public static void Main()
        {
            using (GameHost host = Host.GetSuitableDesktopHost("visual-tests"))
            using (var game = new YouTubePlayerEXTestBrowser())
                host.Run(game);
        }
    }
}
