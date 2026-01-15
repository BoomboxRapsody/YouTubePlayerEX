using System;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.Requests;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using osu.Framework.Configuration;
using YouTubePlayerEX.App.Config;
using YouTubePlayerEX.App.Localisation;

namespace YouTubePlayerEX.App.Online
{
    public partial class YouTubeAPI
    {
        private YouTubeService youtubeService;
        private GoogleTranslate translateApi;

        private FrameworkConfigManager frameworkConfig;
        private YTPlayerEXConfigManager appConfig;

        public YouTubeAPI(FrameworkConfigManager frameworkConfig, GoogleTranslate translateApi, YTPlayerEXConfigManager appConfig)
        {
            this.frameworkConfig = frameworkConfig;
            this.translateApi = translateApi;
            this.appConfig = appConfig;
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

        public IList<CommentThread> GetCommentThread(string videoId, CommentThreadsResource.ListRequest.OrderEnum orderEnum = CommentThreadsResource.ListRequest.OrderEnum.Time)
        {
            var part = "snippet,replies";
            var request = youtubeService.CommentThreads.List(part);

            request.MaxResults = 25; // hell
            request.VideoId = videoId;
            request.Order = orderEnum;

            var response = request.Execute();

            var result = response.Items;

            return result;
        }

        public Comment GetComment(string commentId)
        {
            var part = "snippet";
            var request = youtubeService.Comments.List(part);

            request.Id = commentId;

            var response = request.Execute();

            var result = response.Items.First();

            return result;
        }

        public string GetLocalizedChannelTitle(Channel channel)
        {
            if (channel == null)
                return string.Empty;

            if (appConfig.Get<VideoMetadataTranslateSource>(YTPlayerEXSetting.VideoMetadataTranslateSource) == VideoMetadataTranslateSource.YouTube)
            {
                string language = frameworkConfig.Get<string>(FrameworkSetting.Locale);
                try
                {
                    return channel.Localizations[language].Title;
                }
                catch (Exception e)
                {
                    return channel.Snippet.Title;
                }
            }
            else
            {
                try
                {
                    string originalTitle = channel.Snippet.Title;
                    string translatedTitle = translateApi.Translate(originalTitle, GoogleTranslateLanguage.auto);
                    return translatedTitle;
                }
                catch (Exception e)
                {
                    return channel.Snippet.Title;
                }
            }
        }

        public string ParseCaptionLanguage(ClosedCaptionLanguage captionLanguage)
        {
            switch (captionLanguage)
            {
                case ClosedCaptionLanguage.Disabled:
                {
                    return string.Empty;
                }
                case ClosedCaptionLanguage.English:
                {
                    return "en";
                }
                case ClosedCaptionLanguage.Korean:
                {
                    return "ko";
                }
                case ClosedCaptionLanguage.Japanese:
                {
                    return "ja";
                } 
            }
            return string.Empty;
        }

        public string GetLocalizedVideoTitle(Video video)
        {
            if (video == null)
                return string.Empty;

            if (appConfig.Get<VideoMetadataTranslateSource>(YTPlayerEXSetting.VideoMetadataTranslateSource) == VideoMetadataTranslateSource.GoogleTranslate)
            {
                try
                {
                    string originalTitle = video.Snippet.Title;
                    string translatedTitle = translateApi.Translate(originalTitle, GoogleTranslateLanguage.auto);
                    return translatedTitle;
                }
                catch (Exception e)
                {
                    return video.Snippet.Title;
                }
            }

            string language = frameworkConfig.Get<string>(FrameworkSetting.Locale);
            string languageParsed = string.Empty;

            try
            {
                return video.Localizations[language].Title;
            }
            catch (Exception e)
            {
                return video.Snippet.Title;
            }
        }

        public string GetLocalizedVideoDescription(Video video)
        {
            if (video == null)
                return string.Empty;

            if (appConfig.Get<VideoMetadataTranslateSource>(YTPlayerEXSetting.VideoMetadataTranslateSource) == VideoMetadataTranslateSource.GoogleTranslate)
            {
                try
                {
                    string originalDescription = video.Snippet.Description;
                    string translatedDescription = translateApi.Translate(originalDescription, GoogleTranslateLanguage.auto);
                    return translatedDescription;
                }
                catch (Exception e)
                {
                    return video.Snippet.Description;
                }
            }

            string language = frameworkConfig.Get<string>(FrameworkSetting.Locale);
            string languageParsed = string.Empty;

            try
            {
                return video.Localizations[language].Description;
            }
            catch (Exception e)
            {
                return video.Snippet.Description;
            }
        }

        public Video GetVideo(string videoId)
        {
            var part = "statistics,snippet,localizations,contentDetails,status";
            var request = youtubeService.Videos.List(part);

            request.Id = videoId;

            var response = request.Execute();

            var result = response.Items.First();

            return result;
        }
    }
}
