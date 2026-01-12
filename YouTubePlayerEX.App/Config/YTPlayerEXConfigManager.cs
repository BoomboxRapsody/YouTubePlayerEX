using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Configuration;
using osu.Framework.Platform;

namespace YouTubePlayerEX.App.Config
{
    public class YTPlayerEXConfigManager : IniConfigManager<YTPlayerEXSetting>
    {
        internal const string FILENAME = @"app.ini";

        protected override string Filename => FILENAME;

        protected override void InitialiseDefaults()
        {
            SetDefault(YTPlayerEXSetting.PreferDisplayHandle, true);
            SetDefault(YTPlayerEXSetting.ClosedCaptionLanguage, ClosedCaptionLanguage.Disabled);
            SetDefault(YTPlayerEXSetting.CaptionEnabled, false);
            SetDefault(YTPlayerEXSetting.AspectRatioMethod, AspectRatioMethod.Letterbox);
            SetDefault(YTPlayerEXSetting.VideoMetadataTranslateSource, VideoMetadataTranslateSource.YouTube);
        }

        public YTPlayerEXConfigManager(Storage storage, IDictionary<YTPlayerEXSetting, object> defaultOverrides = null) : base(storage, defaultOverrides)
        {
        }
    }

    public enum YTPlayerEXSetting
    {
        PreferDisplayHandle,
        ClosedCaptionLanguage,
        CaptionEnabled,
        AspectRatioMethod,
        VideoMetadataTranslateSource,
    }
}
