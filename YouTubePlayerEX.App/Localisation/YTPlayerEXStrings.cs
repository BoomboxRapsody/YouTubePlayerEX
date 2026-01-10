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

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
