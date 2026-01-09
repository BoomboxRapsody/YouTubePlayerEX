using osu.Framework.Localisation;

namespace YouTubePlayerEX.App.Localisation
{
    public static class YTPlayerEXStrings
    {
        private const string prefix = @"YouTubePlayerEX.Resources.Localisation.YTPlayerEX";

        /// <summary>
        /// {0} • {1} views
        /// </summary>
        public static LocalisableString VideoMetadataDesc(string username, string views) => new TranslatableString(getKey(@"todays_daily_challenge_has_concluded"), "{0} • {1} views", username, views);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
