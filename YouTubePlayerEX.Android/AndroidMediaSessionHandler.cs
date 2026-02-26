// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using Android.App;
using Android.Media;
using Android.Media.Session;
using Android.OS;
using Google.Apis.YouTube.v3.Data;
using YouTubePlayerEX.App;
using YouTubePlayerEX.App.Online;
using MediaSession = Android.Media.Session.MediaSession;

namespace YouTubePlayerEX.Android
{
    public partial class AndroidMediaSessionHandler : App.MediaSession
    {
        private MediaSession mediaSession;

        private YouTubePlayerEXAppActivity gameActivity;

        private class AndroidMediaSessionCallback : MediaSession.Callback
        {
            private MediaSessionControls? controls;

            public AndroidMediaSessionCallback(MediaSessionControls? controls)
            {
                this.controls = controls;
            }

            public override void OnPlay()
            {
                base.OnPlay();
                controls?.PlayButtonPressed?.Invoke();
            }

            public override void OnPause()
            {
                base.OnPause();
                controls?.PauseButtonPressed?.Invoke();
            }

            public override void OnSkipToNext()
            {
                base.OnSkipToNext();
                controls?.NextButtonPressed?.Invoke();
            }

            public override void OnSkipToPrevious()
            {
                base.OnSkipToPrevious();
                controls?.PrevButtonPressed?.Invoke();
            }
        }

        public AndroidMediaSessionHandler(YouTubePlayerEXAppActivity activity)
            : base()
        {
            gameActivity = activity;
        }

        private MediaSessionControls? controls;

        public override void RegisterControlEvents(MediaSessionControls controls)
        {
            this.controls = controls;
            mediaSession.SetCallback(new AndroidMediaSessionCallback(controls));
        }

        public override void UnregisterControlEvents()
        {
            controls = null;
            mediaSession.SetCallback(new AndroidMediaSessionCallback(null));
        }

        public override void CreateMediaSession(YouTubeAPI youtubeAPI, string audioPath)
        {
            YouTubeAPI = youtubeAPI;

            mediaSession = new MediaSession(gameActivity, "yt-player-ex");

            mediaSession.SetCallback(new AndroidMediaSessionCallback(controls));

            mediaSession.SetFlags(MediaSessionFlags.HandlesMediaButtons | MediaSessionFlags.HandlesTransportControls);

            var stateBuilder = new PlaybackState.Builder();

            stateBuilder.SetActions(
                PlaybackState.ActionPlay |
                PlaybackState.ActionPause |
                PlaybackState.ActionSkipToNext |
                PlaybackState.ActionSkipToPrevious
            );

            stateBuilder.SetState(PlaybackStateCode.Paused, 0, 1.0f);

            mediaSession.SetPlaybackState(stateBuilder.Build());
        }

        public override void UpdateMediaSession(Video video)
        {
            var metadataBuilder = new MediaMetadata.Builder();

            metadataBuilder.PutString(MediaMetadata.MetadataKeyTitle, video.Snippet.Title);
            metadataBuilder.PutString(MediaMetadata.MetadataKeyArtist, video.Snippet.ChannelTitle);

            mediaSession.SetMetadata(metadataBuilder.Build());
        }

        private long position;
        private float playbackSpeed = 1.0f;
        private bool playing;

        public override void UpdatePlayingState(bool playing)
        {
            this.playing = playing;
            var stateBuilder = new PlaybackState.Builder();

            stateBuilder.SetActions(
                PlaybackState.ActionPlay |
                PlaybackState.ActionPause |
                PlaybackState.ActionSkipToNext |
                PlaybackState.ActionSkipToPrevious
            );

            stateBuilder.SetState(playing ? PlaybackStateCode.Playing : PlaybackStateCode.Paused, position, playbackSpeed);

            mediaSession.SetPlaybackState(stateBuilder.Build());

            mediaSession.Active = playing;
        }

        public override void UpdateTimestamp(Video video, double pos)
        {
            position = Convert.ToInt64(pos);
            var stateBuilder = new PlaybackState.Builder();

            stateBuilder.SetActions(
                PlaybackState.ActionPlay |
                PlaybackState.ActionPause |
                PlaybackState.ActionSkipToNext |
                PlaybackState.ActionSkipToPrevious
            );

            stateBuilder.SetState(playing ? PlaybackStateCode.Playing : PlaybackStateCode.Paused, position, playbackSpeed);

            mediaSession.SetPlaybackState(stateBuilder.Build());
        }

        public override void DeleteMediaSession()
        {
            mediaSession.Release();
        }

        public override void UpdatePlaybackSpeed(double speed)
        {
            playbackSpeed = (float)speed;
            var stateBuilder = new PlaybackState.Builder();

            stateBuilder.SetActions(
                PlaybackState.ActionPlay |
                PlaybackState.ActionPause |
                PlaybackState.ActionSkipToNext |
                PlaybackState.ActionSkipToPrevious
            );

            stateBuilder.SetState(playing ? PlaybackStateCode.Playing : PlaybackStateCode.Paused, position, playbackSpeed);

            mediaSession.SetPlaybackState(stateBuilder.Build());
        }
    }
}
