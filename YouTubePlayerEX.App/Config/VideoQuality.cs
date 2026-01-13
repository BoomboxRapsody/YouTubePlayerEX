using System.ComponentModel;
using osu.Framework.Localisation;
using YouTubePlayerEX.App.Localisation;

namespace YouTubePlayerEX.App.Config
{
    public enum VideoQuality
    {
        [LocalisableDescription(typeof(YTPlayerEXStrings), nameof(YTPlayerEXStrings.PreferHighQuality))]
        PreferHighQuality,

        [Description("2160p (4K)")]
        Quality_4K,

        [Description("1440p (QHD)")]
        Quality_1440p,

        [Description("1080p (FHD)")]
        Quality_1080p,

        [Description("720p (HD)")]
        Quality_720p,

        [Description("480p")]
        Quality_480p,

        [Description("360p")]
        Quality_360p,

        [Description("240p")]
        Quality_240p,

        [Description("144p")]
        Quality_144p,
    }
}
