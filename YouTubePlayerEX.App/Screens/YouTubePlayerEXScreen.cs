using osu.Framework.Screens;

namespace YouTubePlayerEX.App.Screens
{
    public abstract partial class YouTubePlayerEXScreen : Screen
    {
        protected new YouTubePlayerEXAppBase Game => base.Game as YouTubePlayerEXAppBase;
    }
}
