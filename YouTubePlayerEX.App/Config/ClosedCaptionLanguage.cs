using System.ComponentModel;
using osu.Framework.Localisation;
using YouTubePlayerEX.App.Localisation;

namespace YouTubePlayerEX.App.Config
{
    public enum ClosedCaptionLanguage
    {
        [LocalisableDescription(typeof(YTPlayerEXStrings), nameof(YTPlayerEXStrings.CaptionDisabled))]
        Disabled,

        [Description("English")]
        English,

        [Description("한국어")]
        Korean,

        [Description("日本語")]
        Japanese,
    }
}
