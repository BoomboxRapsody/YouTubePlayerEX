using osu.Framework.Localisation;

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

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
