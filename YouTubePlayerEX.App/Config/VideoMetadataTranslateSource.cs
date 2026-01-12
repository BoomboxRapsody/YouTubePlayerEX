using System.ComponentModel;
using osu.Framework.Localisation;
using YouTubePlayerEX.App.Localisation;

namespace YouTubePlayerEX.App.Config
{
    public enum VideoMetadataTranslateSource
    {
        YouTube,

        [LocalisableDescription(typeof(YTPlayerEXStrings), nameof(YTPlayerEXStrings.GoogleTranslate))]
        GoogleTranslate
    }
}
