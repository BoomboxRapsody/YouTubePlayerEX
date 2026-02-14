// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework;
using osu.Framework.Configuration;
using osu.Framework.Configuration.Tracking;
using osu.Framework.Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
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
            SetDefault(YTPlayerEXSetting.UsernameDisplayMode, UsernameDisplayMode.Handle);
            SetDefault(YTPlayerEXSetting.ClosedCaptionLanguage, ClosedCaptionLanguage.Disabled);
            SetDefault(YTPlayerEXSetting.CaptionEnabled, false);
            SetDefault(YTPlayerEXSetting.AspectRatioMethod, AspectRatioMethod.Letterbox);
            SetDefault(YTPlayerEXSetting.VideoMetadataTranslateSource, VideoMetadataTranslateSource.YouTube);
            SetDefault(YTPlayerEXSetting.VideoQuality, VideoQuality.PreferHighQuality);
            SetDefault(YTPlayerEXSetting.AudioLanguage, Language.en);
            SetDefault(YTPlayerEXSetting.AdjustPitchOnSpeedChange, false);
            SetDefault(YTPlayerEXSetting.VideoDimLevel, 0, 0, .8, 0.01);
            SetDefault(YTPlayerEXSetting.ShowFpsDisplay, false);
            SetDefault(YTPlayerEXSetting.UIFont, UIFont.Torus);

            SetDefault(YTPlayerEXSetting.Scaling, ScalingMode.Off);
            SetDefault(YTPlayerEXSetting.SafeAreaConsiderations, true);
            SetDefault(YTPlayerEXSetting.ScalingBackgroundDim, 0.9f, 0.5f, 1f, 0.01f);

            SetDefault(YTPlayerEXSetting.ScalingSizeX, 0.8f, 0.2f, 1f, 0.01f);
            SetDefault(YTPlayerEXSetting.ScalingSizeY, 0.8f, 0.2f, 1f, 0.01f);

            SetDefault(YTPlayerEXSetting.ScalingPositionX, 0.5f, 0f, 1f, 0.01f);
            SetDefault(YTPlayerEXSetting.ScalingPositionY, 0.5f, 0f, 1f, 0.01f);

            if (RuntimeInfo.IsMobile)
                SetDefault(YTPlayerEXSetting.UIScale, 1f, 0.8f, 1.1f, 0.01f);
            else
                SetDefault(YTPlayerEXSetting.UIScale, 1f, 0.8f, 1.6f, 0.01f);

            SetDefault(YTPlayerEXSetting.ScreenshotFormat, ScreenshotFormat.Jpg);
            SetDefault(YTPlayerEXSetting.ScreenshotCaptureMenuCursor, true);

            SetDefault(YTPlayerEXSetting.CursorRotation, true);

            SetDefault(YTPlayerEXSetting.FinalLoginState, false);

            SetDefault(YTPlayerEXSetting.AlwaysUseOriginalAudio, false);

            SetDefault(YTPlayerEXSetting.UseSystemCursor, true);

            SetDefault(YTPlayerEXSetting.VideoBloomLevel, 0f, 0f, 1f, 0.01f);
        }

        public YTPlayerEXConfigManager(Storage storage, IDictionary<YTPlayerEXSetting, object> defaultOverrides = null) : base(storage, defaultOverrides)
        {
        }

        public override TrackedSettings CreateTrackedSettings() => new TrackedSettings
        {
            new TrackedSetting<ClosedCaptionLanguage>(YTPlayerEXSetting.ClosedCaptionLanguage, v => new SettingDescription(v, YTPlayerEXStrings.CaptionLanguage, v.GetLocalisableDescription(), "Shift+C")),
            new TrackedSetting<AspectRatioMethod>(YTPlayerEXSetting.AspectRatioMethod, v => new SettingDescription(v, YTPlayerEXStrings.AspectRatioMethod, v.GetLocalisableDescription(), "Ctrl+F6")),
            new TrackedSetting<bool>(YTPlayerEXSetting.AdjustPitchOnSpeedChange, v => new SettingDescription(v, YTPlayerEXStrings.AdjustPitchOnSpeedChange, v == true ? YTPlayerEXStrings.Enabled.ToLower() : YTPlayerEXStrings.Disabled.ToLower(), "Alt+P")),
            new TrackedSetting<float>(YTPlayerEXSetting.UIScale, v => new SettingDescription(v, YTPlayerEXStrings.UIScaling, $@"{v:0.##}x")),
            new TrackedSetting<bool>(YTPlayerEXSetting.ShowFpsDisplay, v => new SettingDescription(v, YTPlayerEXStrings.ShowFPS, v == true ? YTPlayerEXStrings.Enabled.ToLower() : YTPlayerEXStrings.Disabled.ToLower(), "Ctrl+P")),
            new TrackedSetting<ScalingMode>(YTPlayerEXSetting.Scaling, scalingMode => new SettingDescription(
                        rawValue: scalingMode,
                        name: YTPlayerEXStrings.ScreenScaling,
                        value: scalingMode.GetLocalisableDescription(),
                        shortcut: "Ctrl+Shift+F5"
                    )
                ),
        };
    }

    public enum YTPlayerEXSetting
    {
        UsernameDisplayMode,
        ClosedCaptionLanguage,
        CaptionEnabled,
        AspectRatioMethod,
        VideoMetadataTranslateSource,
        VideoQuality,
        UIScale,
        AudioLanguage,
        AdjustPitchOnSpeedChange,
        VideoDimLevel,
        ShowFpsDisplay,
        UIFont,
        Scaling,
        ScalingPositionX,
        ScalingPositionY,
        ScalingSizeX,
        ScalingSizeY,
        ScalingBackgroundDim,
        SafeAreaConsiderations,

        ScreenshotFormat,
        ScreenshotCaptureMenuCursor,

        CursorRotation,

        FinalLoginState,

        AlwaysUseOriginalAudio,
        UseSystemCursor,

        VideoBloomLevel,
    }
}
