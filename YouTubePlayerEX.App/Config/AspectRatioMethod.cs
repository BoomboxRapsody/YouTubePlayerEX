using osu.Framework.Localisation;
using YouTubePlayerEX.App.Localisation;

namespace YouTubePlayerEX.App.Config
{
    public enum AspectRatioMethod
    {
        [LocalisableDescription(typeof(YTPlayerEXStrings), nameof(YTPlayerEXStrings.Letterbox))]
        Letterbox,

        [LocalisableDescription(typeof(YTPlayerEXStrings), nameof(YTPlayerEXStrings.Fill))]
        Fill,
    }
}
