using osu.Framework.Platform;
using osu.Framework;
using YouTubePlayerEX.App;

namespace YouTubePlayerEX.Desktop
{
    public static class Program
    {
        public static void Main()
        {
            using (GameHost host = Host.GetSuitableDesktopHost(@"YouTubePlayerEX"))
            using (osu.Framework.Game game = new YouTubePlayerEXApp())
                host.Run(game);
        }
    }
}
