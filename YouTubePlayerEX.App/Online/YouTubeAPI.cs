using System;
using System.Globalization;
using System.Linq;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using osu.Framework.Configuration;

namespace YouTubePlayerEX.App.Online
{
    public partial class YouTubeAPI
    {
        private YouTubeService youtubeService;

        private FrameworkConfigManager frameworkConfig;

        public YouTubeAPI(FrameworkConfigManager frameworkConfig)
        {
            this.frameworkConfig = frameworkConfig;
            youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = "AIzaSyDGpklqOVqNLzOuChi5hHKswhZIC9ocEIQ",
                ApplicationName = this.GetType().ToString()
            });
        }

        public Channel GetChannel(string channelId)
        {
            var part = "statistics,snippet,brandingSettings,id,localizations";
            var request = youtubeService.Channels.List(part);

            request.Id = channelId;

            var response = request.Execute();

            var result = response.Items.First();

            return result;
        }

        public string GetLocalizedChannelTitle(Channel channel)
        {
            if (channel == null)
                return string.Empty;

            return channel.Snippet.Title;
        }

        public string GetLocalizedVideoTitle(Video video)
        {
            if (video == null)
                return string.Empty;

            string language = frameworkConfig.Get<string>(FrameworkSetting.Locale);
            string languageParsed = string.Empty;

            switch (language)
            {
                case "en":
                {
                    languageParsed = "en-US";
                    break;
                }
                case "ja":
                {
                    languageParsed = "ja-JP";
                    break;
                }
                case "ko":
                {
                    languageParsed = "ko-KR";
                    break;
                }
                default:
                {
                    languageParsed = "en-US";
                    break;
                }
            }

            try
            {
                return video.Localizations[languageParsed].Title;
            }
            catch (Exception e)
            {
                return video.Snippet.Title;
            }
        }

        public Video GetVideo(string videoId)
        {
            var part = "statistics,snippet,localizations";
            var request = youtubeService.Videos.List(part);

            request.Id = videoId;

            var response = request.Execute();

            var result = response.Items.First();

            return result;
        }
    }
}
