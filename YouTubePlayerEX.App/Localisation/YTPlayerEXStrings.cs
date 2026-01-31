// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using YouTubePlayerEX.App.Extensions;

namespace YouTubePlayerEX.App.Localisation
{
    public static class YTPlayerEXStrings
    {
        private const string prefix = @"YouTubePlayerEX.Resources.Localisation.YTPlayerEX";

        /// <summary>
        /// "{0} • {1} views • {2}"
        /// </summary>
        public static LocalisableString VideoMetadataDesc(string username, string views, string daysAgo) => new TranslatableString(getKey(@"video_metadata_desc"), "{0} • {1} views • {2}", username, views, daysAgo);

        /// <summary>
        /// "Playback speed"
        /// </summary>
        public static LocalisableString PlaybackSpeedWithoutValue => new TranslatableString(getKey(@"playback_speed_without_value"), "Playback speed");

        /// <summary>
        /// "Quick Action"
        /// </summary>
        public static LocalisableString QuickAction => new TranslatableString(getKey(@"quick_action"), "Quick Action");

        /// <summary>
        /// "Export logs"
        /// </summary>
        public static LocalisableString ExportLogs => new TranslatableString(getKey(@"export_logs"), "Export logs");

        /// <summary>
        /// "Load from video ID"
        /// </summary>
        public static LocalisableString LoadFromVideoId => new TranslatableString(getKey(@"load_from_video_id"), "Load from video ID");

        /// <summary>
        /// "Load Video"
        /// </summary>
        public static LocalisableString LoadVideo => new TranslatableString(getKey(@"load_video"), "Load Video");

        /// <summary>
        /// "Video ID must not be empty!"
        /// </summary>
        public static LocalisableString NoVideoIdError => new TranslatableString(getKey(@"error_noVideoId"), "Video ID must not be empty!");

        /// <summary>
        /// "{0} • {1} subscribers • Click to view channel via external web browser."
        /// </summary>
        public static LocalisableString ProfileImageTooltip(string username, string subs) => new TranslatableString(getKey(@"profile_image_tooltip"), "{0} • {1} subscribers • Click to view channel via external web browser.", username, subs);

        /// <summary>
        /// "Settings"
        /// </summary>
        public static LocalisableString Settings => new TranslatableString(getKey(@"settings"), "Settings");

        /// <summary>
        /// "Screen resolution"
        /// </summary>
        public static LocalisableString ScreenResolution => new TranslatableString(getKey(@"screen_resolution"), "Screen resolution");

        /// <summary>
        /// "Disabled"
        /// </summary>
        public static LocalisableString CaptionDisabled => new TranslatableString(getKey(@"caption_disabled"), "Disabled");

        /// <summary>
        /// "General"
        /// </summary>
        public static LocalisableString General => new TranslatableString(getKey(@"general"), "General");

        /// <summary>
        /// "Graphics"
        /// </summary>
        public static LocalisableString Graphics => new TranslatableString(getKey(@"graphics"), "Graphics");

        /// <summary>
        /// "Language"
        /// </summary>
        public static LocalisableString Language => new TranslatableString(getKey(@"language"), "Language");

        /// <summary>
        /// "Display"
        /// </summary>
        public static LocalisableString Display => new TranslatableString(getKey(@"display"), "Display");

        /// <summary>
        /// "Screen mode"
        /// </summary>
        public static LocalisableString ScreenMode => new TranslatableString(getKey(@"screen_mode"), "Screen mode");

        /// <summary>
        /// "Closed caption language (only available)"
        /// </summary>
        public static LocalisableString CaptionLanguage => new TranslatableString(getKey(@"caption_language"), "Closed caption language (only available)");

        /// <summary>
        /// "Closed caption font"
        /// </summary>
        public static LocalisableString CaptionFont => new TranslatableString(getKey(@"caption_font"), "Closed caption font");

        /// <summary>
        /// "Press F11 to exit the full screen."
        /// </summary>
        public static LocalisableString FullscreenEntered => new TranslatableString(getKey(@"fullscreen_entered"), "Press F11 to exit the full screen.");

        /// <summary>
        /// "Audio"
        /// </summary>
        public static LocalisableString Audio => new TranslatableString(getKey(@"audio"), "Audio");

        /// <summary>
        /// "Video volume"
        /// </summary>
        public static LocalisableString VideoVolume => new TranslatableString(getKey(@"video_volume"), "Video volume");

        /// <summary>
        /// "SFX volume"
        /// </summary>
        public static LocalisableString SFXVolume => new TranslatableString(getKey(@"sfx_volume"), "SFX volume");

        /// <summary>
        /// "Selected caption: {0}"
        /// </summary>
        public static LocalisableString SelectedCaption(LocalisableString language) => new TranslatableString(getKey(@"selected_caption"), "Selected caption: {0}", language);

        /// <summary>
        /// "Selected caption: {0} (auto-generated)"
        /// </summary>
        public static LocalisableString SelectedCaptionAutoGen(LocalisableString language) => new TranslatableString(getKey(@"selected_caption_auto_gen"), "Selected caption: {0} (auto-generated)", language);

        /// <summary>
        /// "{0} (auto-generated)"
        /// </summary>
        public static LocalisableString CaptionAutoGen(LocalisableString language) => new TranslatableString(getKey(@"caption_auto_gen"), "{0} (auto-generated)", language);

        /// <summary>
        /// "Fill"
        /// </summary>
        public static LocalisableString Fill => new TranslatableString(getKey(@"fill"), "Fill");

        /// <summary>
        /// "Letterbox"
        /// </summary>
        public static LocalisableString Letterbox => new TranslatableString(getKey(@"letterbox"), "Letterbox");

        /// <summary>
        /// "Aspect ratio method"
        /// </summary>
        public static LocalisableString AspectRatioMethod => new TranslatableString(getKey(@"aspect_ratio_method"), "Aspect ratio method");

        /// <summary>
        /// "Estimated: {0} | Actual: {1}"
        /// </summary>
        public static LocalisableString DislikeCountTooltip(string estimated, string actual) => new TranslatableString(getKey(@"dislike_count_tooltip"), "Estimated: {0} | Actual: {1}", estimated, actual);

        /// <summary>
        /// "Video metadata translate source"
        /// </summary>
        public static LocalisableString VideoMetadataTranslateSource => new TranslatableString(getKey(@"video_metadata_translate_source"), "Video metadata translate source");

        /// <summary>
        /// "Google Translate"
        /// </summary>
        public static LocalisableString GoogleTranslate => new TranslatableString(getKey(@"google_translate"), "Google Translate");

        /// <summary>
        /// "Auto"
        /// </summary>
        public static LocalisableString Auto => new TranslatableString(getKey(@"auto"), "Auto");

        /// <summary>
        /// "Video"
        /// </summary>
        public static LocalisableString Video => new TranslatableString(getKey(@"video"), "Video");

        /// <summary>
        /// "Use hardware acceleration"
        /// </summary>
        public static LocalisableString UseHardwareAcceleration => new TranslatableString(getKey(@"use_hardware_acceleration"), @"Use hardware acceleration");

        /// <summary>
        /// "Minimise video player when switching to another app"
        /// </summary>
        public static LocalisableString MinimiseOnFocusLoss => new TranslatableString(getKey(@"minimise_on_focus_loss"), @"Minimise video player when switching to another app");

        /// <summary>
        /// "Prefer high quality"
        /// </summary>
        public static LocalisableString PreferHighQuality => new TranslatableString(getKey(@"prefer_high_quality"), "Prefer high quality");

        /// <summary>
        /// "Video quality"
        /// </summary>
        public static LocalisableString VideoQuality => new TranslatableString(getKey(@"video_quality"), "Video quality");

        /// <summary>
        /// "Master volume"
        /// </summary>
        public static LocalisableString MasterVolume => new TranslatableString(getKey(@"master_volume"), "Master volume");

        /// <summary>
        /// "Enabled"
        /// </summary>
        public static LocalisableString Enabled => new TranslatableString(getKey(@"enabled"), "Enabled");

        /// <summary>
        /// "Disabled"
        /// </summary>
        public static LocalisableString Disabled => new TranslatableString(getKey(@"disabled"), "Disabled");

        /// <summary>
        /// "UI scaling"
        /// </summary>
        public static LocalisableString UIScaling => new TranslatableString(getKey(@"ui_scaling"), @"UI scaling");

        /// <summary>
        /// "Audio tracks"
        /// </summary>
        public static LocalisableString AudioLanguage => new TranslatableString(getKey(@"audio_language"), @"Audio tracks");

        /// <summary>
        /// "Adjust pitch on speed change"
        /// </summary>
        public static LocalisableString AdjustPitchOnSpeedChange => new TranslatableString(getKey(@"adjust_pitch_on_speed_change"), @"Adjust pitch on speed change");

        /// <summary>
        /// "{0} views  {1}"
        /// </summary>
        public static LocalisableString VideoMetadataDescWithoutChannelName(string views, string daysAgo) => new TranslatableString(getKey(@"video_metadata_desc_without_channel_name"), "{0} views  {1}", views, daysAgo);

        /// <summary>
        /// "Comments ({0})"
        /// </summary>
        public static LocalisableString Comments(LocalisableString count) => new TranslatableString(getKey(@"comments"), "Comments ({0})", count);

        /// <summary>
        /// "Video dim level"
        /// </summary>
        public static LocalisableString VideoDimLevel => new TranslatableString(getKey(@"dim"), "Video dim level");

        /// <summary>
        /// "Translate to {0}"
        /// </summary>
        public static LocalisableString TranslateTo(LocalisableString targetLang) => new TranslatableString(getKey(@"translate_to"), "Translate to {0}", targetLang);

        /// <summary>
        /// "See original (Translated by Google)"
        /// </summary>
        public static LocalisableString TranslateViewOriginal => new TranslatableString(getKey(@"translate_view_original"), "See original (Translated by Google)");

        /// <summary>
        /// "{0} (Reply to {1})"
        /// </summary>
        public static LocalisableString CommentReply(string from, string to) => new TranslatableString(getKey(@"comment_reply"), "{0} (Reply to {1})", from, to);

        /// <summary>
        /// "You are running the latest release ({0})"
        /// </summary>
        public static LocalisableString RunningLatestRelease(string version) => new TranslatableString(getKey(@"running_latest_release"), @"You are running the latest release ({0})", version);

        /// <summary>
        /// "Downloading update... {0}%"
        /// </summary>
        public static LocalisableString DownloadingUpdate(string percentage) => new TranslatableString(getKey(@"updating"), @"Downloading update... {0}%", percentage);

        /// <summary>
        /// "To apply updates, please restart the app."
        /// </summary>
        public static LocalisableString RestartRequired => new TranslatableString(getKey(@"restart_required"), "To apply updates, please restart the app.");

        /// <summary>
        /// "Update failed!"
        /// </summary>
        public static LocalisableString UpdateFailed => new TranslatableString(getKey(@"update_failed"), "Update failed!");

        /// <summary>
        /// "Checking for update..."
        /// </summary>
        public static LocalisableString CheckingUpdate => new TranslatableString(getKey(@"checking_update"), "Checking for update...");

        /// <summary>
        /// "Check for updates"
        /// </summary>
        public static LocalisableString CheckUpdate => new TranslatableString(getKey(@"check_update"), "Check for updates");

        /// <summary>
        /// "Frame limiter"
        /// </summary>
        public static LocalisableString FrameLimiter => new TranslatableString(getKey(@"frame_limiter"), "Frame limiter");

        /// <summary>
        /// "Like count hidden by uploader"
        /// </summary>
        public static LocalisableString LikeCountHidden => new TranslatableString(getKey(@"like_count_hidden"), "Like count hidden by uploader");

        /// <summary>
        /// "Disabled by uploader"
        /// </summary>
        public static LocalisableString DisabledByUploader => new TranslatableString(getKey(@"disabled_by_uploader"), "Disabled by uploader");

        /// <summary>
        /// "Dislike count data is provided by the Return YouTube Dislike API."
        /// </summary>
        public static LocalisableString DislikeCounterCredits => new TranslatableString(getKey(@"dislike_counter_credits"), @"Dislike count data is provided by the Return YouTube Dislike API.");

        /// <summary>
        /// "Default"
        /// </summary>
        public static LocalisableString Default => new TranslatableString(getKey(@"common_default"), "Default");

        /// <summary>
        /// "Show FPS"
        /// </summary>
        public static LocalisableString ShowFPS => new TranslatableString(getKey(@"show_fps"), "Show FPS");

        /// <summary>
        /// "Cannot play private videos."
        /// </summary>
        public static LocalisableString CannotPlayPrivateVideos => new TranslatableString(getKey(@"cannot_play_private_videos"), "Cannot play private videos.");

        /// <summary>
        /// "Play"
        /// </summary>
        public static LocalisableString Play => new TranslatableString(getKey(@"play"), "Play");

        /// <summary>
        /// "Pause"
        /// </summary>
        public static LocalisableString Pause => new TranslatableString(getKey(@"pause"), "Pause");

        /// <summary>
        /// "Playback speed: {0}"
        /// </summary>
        public static LocalisableString PlaybackSpeed(double value) => new TranslatableString(getKey(@"playback_speed"), "Playback speed: {0}", value.ToStandardFormattedString(5, true));

        /// <summary>
        /// "Comments"
        /// </summary>
        public static LocalisableString CommentsWithoutCount => new TranslatableString(getKey(@"comments_without_count"), "Comments");

        /// <summary>
        /// "Setting the video quality to 8K may cause performance degradation, GPU overload, and driver crashes on some devices."
        /// </summary>
        public static LocalisableString VideoQuality8KWarning => new TranslatableString(getKey(@"video_quality_8k_warning"), "Setting the video quality to 8K may cause performance degradation, GPU overload, and driver crashes on some devices.");

        /// <summary>
        /// "Renderer"
        /// </summary>
        public static LocalisableString Renderer => new TranslatableString(getKey(@"renderer"), @"Renderer");

        /// <summary>
        /// "Exported logs!"
        /// </summary>
        public static LocalisableString LogsExportFinished => new TranslatableString(getKey(@"logs_export_finished"), @"Exported logs!");

        /// <summary>
        /// "Revert to default"
        /// </summary>
        public static LocalisableString RevertToDefault => new TranslatableString(getKey(@"revert_to_default"), @"Revert to default");

        /// <summary>
        /// "Everything"
        /// </summary>
        public static LocalisableString ScaleEverything => new TranslatableString(getKey(@"scale_everything"), @"Everything");

        /// <summary>
        /// "Video"
        /// </summary>
        public static LocalisableString ScaleVideo => new TranslatableString(getKey(@"scale_video"), @"Video");

        /// <summary>
        /// "Off"
        /// </summary>
        public static LocalisableString ScalingOff => new TranslatableString(getKey(@"scaling_off"), @"Off");

        /// <summary>
        /// "Screen scaling"
        /// </summary>
        public static LocalisableString ScreenScaling => new TranslatableString(getKey(@"screen_scaling"), @"Screen scaling");

        /// <summary>
        /// "Horizontal position"
        /// </summary>
        public static LocalisableString HorizontalPosition => new TranslatableString(getKey(@"horizontal_position"), @"Horizontal position");

        /// <summary>
        /// "Vertical position"
        /// </summary>
        public static LocalisableString VerticalPosition => new TranslatableString(getKey(@"vertical_position"), @"Vertical position");

        /// <summary>
        /// "Horizontal scale"
        /// </summary>
        public static LocalisableString HorizontalScale => new TranslatableString(getKey(@"horizontal_scale"), @"Horizontal scale");

        /// <summary>
        /// "Vertical scale"
        /// </summary>
        public static LocalisableString VerticalScale => new TranslatableString(getKey(@"vertical_scale"), @"Vertical scale");

        /// <summary>
        /// "Thumbnail dim"
        /// </summary>
        public static LocalisableString ThumbnailDim => new TranslatableString(getKey(@"thumbnail_dim"), @"Thumbnail dim");

        /// <summary>
        /// "Username display mode"
        /// </summary>
        public static LocalisableString UsernameDisplayMode => new TranslatableString(getKey(@"username_display_mode"), @"Username display mode");

        /// <summary>
        /// "Shrink app to avoid cameras and notches"
        /// </summary>
        public static LocalisableString ShrinkGameToSafeArea => new TranslatableString(getKey(@"shrink_game_to_safe_area"), @"Shrink app to avoid cameras and notches");

        /// <summary>
        /// "JPG (web-friendly)"
        /// </summary>
        public static LocalisableString Jpg => new TranslatableString(getKey(@"jpg_web_friendly"), @"JPG (web-friendly)");

        /// <summary>
        /// "PNG (lossless)"
        /// </summary>
        public static LocalisableString Png => new TranslatableString(getKey(@"png_lossless"), @"PNG (lossless)");

        /// <summary>
        /// "Screenshot saved!"
        /// </summary>
        public static LocalisableString ScreenshotSaved => new TranslatableString(getKey(@"screenshot_saved"), @"Screenshot saved!");

        /// <summary>
        /// "Screenshot"
        /// </summary>
        public static LocalisableString Screenshot => new TranslatableString(getKey(@"screenshot"), @"Screenshot");

        /// <summary>
        /// "Screenshot format"
        /// </summary>
        public static LocalisableString ScreenshotFormat => new TranslatableString(getKey(@"screenshot_format"), @"Screenshot format");

        /// <summary>
        /// "Show user interface in screenshots"
        /// </summary>
        public static LocalisableString ShowCursorInScreenshots => new TranslatableString(getKey(@"show_cursor_in_screenshots"), @"Show user interface in screenshots");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
