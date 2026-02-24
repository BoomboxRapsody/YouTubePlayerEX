// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using Google.Apis.YouTube.v3.Data;
using YouTubePlayerEX.App.Online;

namespace YouTubePlayerEX.App
{
    public abstract class MediaSession
    {
        protected YouTubeAPI YouTubeAPI;

        public abstract void CreateMediaSession(YouTubeAPI youtubeAPI, string audioPath);

        public abstract void UpdateMediaSession(Video video);

        public abstract void UpdateTimestamp(Video video, double pos);

        public abstract void DeleteMediaSession();

        public abstract void RegisterControlEvents(MediaSessionControls controls);

        public abstract void UnregisterControlEvents();

        public abstract void UpdatePlaybackSpeed(double speed);

        public abstract void UpdatePlayingState(bool playing);
    }

    public class MediaSessionControls
    {
        public Action PlayButtonPressed;
        public Action PauseButtonPressed;
        public Action PrevButtonPressed;
        public Action NextButtonPressed;
    } 
}
