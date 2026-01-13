using osu.Framework.Localisation;
using static osuTK.Graphics.OpenGL.GL;

namespace YouTubePlayerEX.App.Localisation
{
    public static class YTPlayerEXStrings
    {
        private const string prefix = @"YouTubePlayerEX.Resources.Localisation.YTPlayerEX";

        /// <summary>
        /// "{0} • {1} views"
        /// </summary>
        public static LocalisableString VideoMetadataDesc(string username, string views) => new TranslatableString(getKey(@"videoMetadata_desc"), "{0} • {1} views", username, views);

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

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
