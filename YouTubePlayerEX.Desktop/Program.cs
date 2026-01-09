using osu.Framework.Platform;
using osu.Framework;
using YouTubePlayerEX.App;

namespace YouTubePlayerEX.Desktop
{
    public static class Program
    {
        public static void Main()
        {
            HostOptions hostOptions = new HostOptions
            {
                FriendlyGameName = "YouTube Player EX"
            };

            using (GameHost host = Host.GetSuitableDesktopHost(@"YouTubePlayerEX", hostOptions))
            using (osu.Framework.Game game = new YouTubePlayerEXApp())
                host.Run(game);
        }
    }
}
