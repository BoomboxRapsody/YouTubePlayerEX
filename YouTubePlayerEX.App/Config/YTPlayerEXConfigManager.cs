using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework;
using osu.Framework.Configuration;
using osu.Framework.Platform;
using YouTubePlayerEX.App.Localisation;

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
            SetDefault(YTPlayerEXSetting.VideoQuality, VideoQuality.PreferHighQuality);
            SetDefault(YTPlayerEXSetting.AudioLanguage, Language.en);
            SetDefault(YTPlayerEXSetting.AdjustPitchOnSpeedChange, false);

            if (RuntimeInfo.IsMobile)
                SetDefault(YTPlayerEXSetting.UIScale, 1f, 0.8f, 1.1f, 0.01f);
            else
                SetDefault(YTPlayerEXSetting.UIScale, 1f, 0.8f, 1.6f, 0.01f);
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
        VideoQuality,
        UIScale,
        AudioLanguage,
        AdjustPitchOnSpeedChange,
    }
}
