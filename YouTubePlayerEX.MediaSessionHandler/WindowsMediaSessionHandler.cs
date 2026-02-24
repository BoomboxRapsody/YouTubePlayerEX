// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using Google.Apis.YouTube.v3.Data;
using osu.Framework.Extensions;
using osu.Framework.Logging;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using YouTubePlayerEX.App;
using YouTubePlayerEX.App.Online;

namespace YouTubePlayerEX.MediaSessionHandler
{
    public class WindowsMediaSessionHandler : MediaSession
    {
        private SystemMediaTransportControls smtc;
        private MediaPlayer mediaPlayer;

#nullable enable
        private MediaSessionControls? controls;
#nullable disable

        public override void CreateMediaSession(YouTubeAPI youtubeAPI, string audioPath)
        {
            Task.Run(async () =>
            {
                mediaPlayer = new MediaPlayer
                {
                    Source = MediaSource.CreateFromUri(new Uri($"file:///{audioPath}")),
                    Volume = 0,
                };

                smtc = mediaPlayer.SystemMediaTransportControls;
                smtc.PlaybackStatus = MediaPlaybackStatus.Paused;
                smtc.IsEnabled = true;
                smtc.IsPlayEnabled = true;
                smtc.IsPauseEnabled = true;
                smtc.IsNextEnabled = true;
                smtc.IsPreviousEnabled = true;
                smtc.IsFastForwardEnabled = true;
                smtc.IsRewindEnabled = true;

                smtc.ButtonPressed += smtc_ButtonPressed;

                smtc.DisplayUpdater.Type = Windows.Media.MediaPlaybackType.Music;

                smtc.DisplayUpdater.MusicProperties.Title = "(unknown)";
                smtc.DisplayUpdater.MusicProperties.Artist = "(unknown)";

                smtc.DisplayUpdater.Update();

                base.YouTubeAPI = youtubeAPI;
            });
        }

        public override void UpdateMediaSession(Video video)
        {
            Task.Run(async () =>
            {
                smtc.DisplayUpdater.MusicProperties.Title = video.Snippet.Title;
                smtc.DisplayUpdater.MusicProperties.Artist = YouTubeAPI.GetChannel(video.Snippet.ChannelId).Snippet.Title;
                smtc.DisplayUpdater.Thumbnail = Windows.Storage.Streams.RandomAccessStreamReference.CreateFromUri(new Uri(video.Snippet.Thumbnails.High.Url));

                smtc.DisplayUpdater.Update();
            });
        }

        public override void UpdatePlayingState(bool playing)
        {
            Task.Run(async () =>
            {
                smtc.PlaybackStatus = playing ? MediaPlaybackStatus.Playing : MediaPlaybackStatus.Paused;
            });
        }

        public override void UpdateTimestamp(Video video, double pos)
        {
            try
            {
                /*
                smtc.UpdateTimelineProperties(new SystemMediaTransportControlsTimelineProperties
                {
                    StartTime = TimeSpan.Zero,
                    EndTime = XmlConvert.ToTimeSpan(video.ContentDetails.Duration),
                    Position = TimeSpan.FromSeconds(pos)
                });
                */

                mediaPlayer.Position = TimeSpan.FromSeconds(pos * 0.001f);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.GetDescription());
            }
        }

        public override void DeleteMediaSession()
        {
            smtc.DisplayUpdater.ClearAll();
        }

        private void smtc_ButtonPressed(Windows.Media.SystemMediaTransportControls sender, Windows.Media.SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case Windows.Media.SystemMediaTransportControlsButton.Play:
                    controls?.PlayButtonPressed?.Invoke();
                    break;
                case Windows.Media.SystemMediaTransportControlsButton.Pause:
                    controls?.PauseButtonPressed?.Invoke();
                    break;
                case Windows.Media.SystemMediaTransportControlsButton.Rewind:
                    controls?.PrevButtonPressed?.Invoke();
                    break;
                case Windows.Media.SystemMediaTransportControlsButton.FastForward:
                    controls?.NextButtonPressed?.Invoke();
                    break;
                case Windows.Media.SystemMediaTransportControlsButton.Previous:
                    controls?.PrevButtonPressed?.Invoke();
                    break;
                case Windows.Media.SystemMediaTransportControlsButton.Next:
                    controls?.NextButtonPressed?.Invoke();
                    break;
            }
        }

        public override void RegisterControlEvents(MediaSessionControls controls)
        {
            this.controls = controls;
        }

        public override void UnregisterControlEvents()
        {
            controls = null;
        }

        public override void UpdatePlaybackSpeed(double speed)
        {
            smtc.PlaybackRate = speed;
            mediaPlayer.PlaybackRate = speed;
        }
    }
}
