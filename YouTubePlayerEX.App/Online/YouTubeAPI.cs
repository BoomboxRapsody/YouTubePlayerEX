// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Crypto.AES;
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
        private GoogleOAuth2 googleOAuth2;

        private FrameworkConfigManager frameworkConfig;
        private YTPlayerEXConfigManager appConfig;

        public YouTubeAPI(FrameworkConfigManager frameworkConfig, GoogleTranslate translateApi, YTPlayerEXConfigManager appConfig, GoogleOAuth2 googleOAuth2, bool isTestClient)
        {
            this.frameworkConfig = frameworkConfig;
            this.translateApi = translateApi;
            this.appConfig = appConfig;
            this.googleOAuth2 = googleOAuth2;
            var apiKey = isTestClient ? "K/1395zhx/B49AZcHQpAUn5HZSBGtbLrAHnY3QGYieBQpx0gOkZdL5xDPUB7+BnM" : "3T8gSwQR7sprXV/OZDZyTCqbT9Qrt/j8xd7prlHrFMh4Y8Dsp4H2HG+eu+UJ7FOb";

            using (AES aes = new AES("apiKey"))
            {
                string decryptedApiKey = aes.Decrypt(apiKey);

                youtubeService = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = decryptedApiKey,
                    ApplicationName = GetType().ToString()
                });
            }
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

        public Channel TryToGetMineChannel()
        {
            if (!googleOAuth2.SignedIn.Value)
                return null;

            return GetMineChannel();
        }

        public Channel GetMineChannel()
        {
            var part = "statistics,snippet,brandingSettings,id,localizations";
            var request = youtubeService.Channels.List(part);

            request.AccessToken = googleOAuth2.GetAccessToken();

            request.Mine = true;

            var response = request.Execute();

            var result = response.Items.First();

            return result;
        }

        public void SendComment(string videoId, string commentText)
        {
            if (!googleOAuth2.SignedIn.Value)
                return;

            var part = "snippet";
            var request = youtubeService.CommentThreads.Insert(new CommentThread
            {
                Snippet = new CommentThreadSnippet
                {
                    VideoId = videoId,
                    TopLevelComment = new Comment
                    {
                        Snippet = new CommentSnippet
                        {
                            TextOriginal = commentText
                        }
                    }
                }
            }, part);

            request.AccessToken = googleOAuth2.GetAccessToken();

            request.Execute();
        }

        public IList<CommentThread> GetCommentThread(string videoId, CommentThreadsResource.ListRequest.OrderEnum orderEnum = CommentThreadsResource.ListRequest.OrderEnum.Time)
        {
            var part = "snippet,replies";
            var request = youtubeService.CommentThreads.List(part);

            request.MaxResults = 20; // <------ why 20? dues to quota limits
            request.VideoId = videoId;
            request.Order = orderEnum;

            var response = request.Execute();

            var result = response.Items;

            return result;
        }

        public IList<SearchResult> GetSearchResult(string query)
        {
            var part = "snippet";
            var request = youtubeService.Search.List(part);

            request.MaxResults = 20; // <------ why 20? dues to quota limits
            request.Q = query;

            var response = request.Execute();

            var result = response.Items;

            return result;
        }

        public async Task<Comment> GetComment(string commentId)
        {
            var part = "snippet";
            var request = youtubeService.Comments.List(part);

            request.Id = commentId;

            var response = await request.ExecuteAsync();

            var result = response.Items.First();

            return result;
        }

        public string GetLocalizedChannelTitle(Channel channel, bool displayBoth = false)
        {
            if (channel == null)
                return string.Empty;

            if (displayBoth)
            {
                if (appConfig.Get<VideoMetadataTranslateSource>(YTPlayerEXSetting.VideoMetadataTranslateSource) == VideoMetadataTranslateSource.YouTube)
                {
                    string language = frameworkConfig.Get<string>(FrameworkSetting.Locale);
                    try
                    {
                        return channel.Localizations[language].Title + $" ({channel.Snippet.CustomUrl})";
                    }
                    catch
                    {
                        return channel.Snippet.Title + $" ({channel.Snippet.CustomUrl})";
                    }
                }
                else
                {
                    try
                    {
                        string originalTitle = channel.Snippet.Title;
                        string translatedTitle = translateApi.Translate(originalTitle, GoogleTranslateLanguage.auto);
                        return translatedTitle + $" ({channel.Snippet.CustomUrl})";
                    }
                    catch
                    {
                        return channel.Snippet.Title + $" ({channel.Snippet.CustomUrl})";
                    }
                }
            }

            if (appConfig.Get<UsernameDisplayMode>(YTPlayerEXSetting.UsernameDisplayMode) == UsernameDisplayMode.DisplayName)
            {
                if (appConfig.Get<VideoMetadataTranslateSource>(YTPlayerEXSetting.VideoMetadataTranslateSource) == VideoMetadataTranslateSource.YouTube)
                {
                    string language = frameworkConfig.Get<string>(FrameworkSetting.Locale);
                    try
                    {
                        return channel.Localizations[language].Title;
                    }
                    catch
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
                    catch
                    {
                        return channel.Snippet.Title;
                    }
                }
            }
            else
            {
                return channel.Snippet.CustomUrl;
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
                catch
                {
                    return video.Snippet.Title;
                }
            }

            string language = frameworkConfig.Get<string>(FrameworkSetting.Locale);

            try
            {
                return video.Localizations[language].Title;
            }
            catch
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
                catch
                {
                    return video.Snippet.Description;
                }
            }

            string language = frameworkConfig.Get<string>(FrameworkSetting.Locale);

            try
            {
                return video.Localizations[language].Description;
            }
            catch
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
