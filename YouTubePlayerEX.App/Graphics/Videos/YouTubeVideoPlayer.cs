// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Video;
using osu.Framework.Timing;
using YoutubeExplode.Videos.ClosedCaptions;
using YouTubePlayerEX.App.Config;
using YouTubePlayerEX.App.Graphics.Caption;
using YouTubePlayerEX.App.Graphics.Containers;

namespace YouTubePlayerEX.App.Graphics.Videos
{
    public partial class YouTubeVideoPlayer : Container
    {
        private Video video = null!;
        private Track track = null!;
        private DrawableTrack drawableTrack = null!;

        private string fileName_Video, fileName_Audio = null!;
        private ClosedCaptionTrack captionTrack = null!;
        private ClosedCaptionLanguage captionLanguage;

        private StopwatchClock rateAdjustClock = null!;
        private DecouplingFramedClock framedClock = null!;

        private Bindable<double> playbackSpeed = null!;
        private double resumeFromTime;
        private bool trackFinished = false;

        public YouTubeVideoPlayer(string fileName_Video, string fileName_Audio, ClosedCaptionTrack captionTrack, ClosedCaptionLanguage captionLanguage, double resumeFromTime)
        {
            this.fileName_Video = fileName_Video;
            this.fileName_Audio = fileName_Audio;
            this.captionTrack = captionTrack;
            this.captionLanguage = captionLanguage;
            this.resumeFromTime = resumeFromTime;
        }

        public void UpdateCaptionTrack(ClosedCaptionLanguage captionLanguage, ClosedCaptionTrack captionTrack)
        {
            this.captionTrack = captionTrack;
            closedCaption.UpdateCaptionTrack(captionLanguage, captionTrack);
        }

        public BindableNumber<double> VideoProgress = new BindableNumber<double>()
        {
            MinValue = 0,
            MaxValue = 1,
        };

        private KeyBindingAnimations keyBindingAnimations = null!;
        private ClosedCaptionContainer closedCaption = null!;
        private Bindable<AspectRatioMethod> aspectRatioMethod = null!;

        [BackgroundDependencyLoader]
        private void load(ITrackStore tracks, YTPlayerEXConfigManager config)
        {
            aspectRatioMethod = config.GetBindable<AspectRatioMethod>(YTPlayerEXSetting.AspectRatioMethod);
            track = tracks.GetFromStream(File.OpenRead(fileName_Audio), fileName_Audio);
            playbackSpeed = new Bindable<double>(1);

            rateAdjustClock = new StopwatchClock(true);
            framedClock = new DecouplingFramedClock(rateAdjustClock);

            AddRange(new Drawable[] {
                drawableTrack = new DrawableTrack(track)
                {
                    Clock = framedClock,
                },
                new DimmableContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = video = new Video(fileName_Video, false)
                    {
                        RelativeSizeAxes = Axes.Both,
                        FillMode = FillMode.Fit,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Clock = framedClock,
                    },
                },
                keyBindingAnimations = new KeyBindingAnimations
                {
                    RelativeSizeAxes = Axes.Both,
                },
                closedCaption = new ClosedCaptionContainer(this, captionTrack, captionLanguage)
            });

            rateAdjustClock.Rate = playbackSpeed.Value;

            playbackSpeed.BindValueChanged(v =>
            {
                rateAdjustClock.Rate = v.NewValue;
            });

            UpdatePreservePitch(config.Get<bool>(YTPlayerEXSetting.AdjustPitchOnSpeedChange));

            drawableTrack.Completed += trackCompleted;

            Play();

            aspectRatioMethod.BindValueChanged(value =>
            {
                video.FillMode = value.NewValue == AspectRatioMethod.Letterbox ? FillMode.Fit : FillMode.Stretch;
            }, true);
        }

        public void UpdateControlsVisibleState(bool state)
        {
            closedCaption.UpdateControlsVisibleState(state);
        }

        private void trackCompleted()
        {
            trackFinished = true;
            Pause();
        }

        public void UpdatePreservePitch(bool value)
        {
            drawableTrack?.RemoveAllAdjustments(AdjustableProperty.Tempo);
            drawableTrack?.RemoveAllAdjustments(AdjustableProperty.Frequency);

            if (value == true)
                drawableTrack?.AddAdjustment(AdjustableProperty.Frequency, playbackSpeed);
            else
                drawableTrack?.AddAdjustment(AdjustableProperty.Tempo, playbackSpeed);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            drawableTrack.Dispose();
            video.Dispose();
        }

        public bool IsPlaying()
        {
            if (drawableTrack == null)
                return false;

            if (drawableTrack.HasCompleted)
                return false;

            return drawableTrack.IsRunning;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (resumeFromTime != 0)
                SeekTo(resumeFromTime);
        }

        private bool paused;

        protected override void Update()
        {
            base.Update();

            if (drawableTrack != null)
            {
                VideoProgress.MaxValue = drawableTrack.Length / 1000;
                VideoProgress.Value = drawableTrack.CurrentTime / 1000;
            }
        }

        public void SeekTo(double pos)
        {
            double pos2 = pos;

            if (pos2 < 0)
                pos2 = 0;

            drawableTrack?.Seek(pos2);
            video?.Seek(pos2);
        }

        public void FastForward10Sec()
        {
            if ((drawableTrack.CurrentTime + 10000) >= drawableTrack.Length)
            {
                SeekTo(drawableTrack.Length);
                trackFinished = true;
                Pause();
            }

            SeekTo(drawableTrack.CurrentTime + 10000);
            keyBindingAnimations.PlaySeekAnimation(KeyBindingAnimations.SeekAction.FastForward10sec, FontAwesome.Solid.Box);
        }

        public void FastRewind10Sec()
        {
            SeekTo(drawableTrack.CurrentTime - 10000);
            keyBindingAnimations.PlaySeekAnimation(KeyBindingAnimations.SeekAction.FastRewind10sec, FontAwesome.Solid.Box);
        }

        public void Pause(bool isKeyboardAction = false)
        {
            drawableTrack?.Stop();
            framedClock.Stop();

            if (isKeyboardAction)
                keyBindingAnimations.PlaySeekAnimation(KeyBindingAnimations.SeekAction.PlayPause, FontAwesome.Solid.Pause);

            paused = true;
        }

        public void Play(bool isKeyboardAction = false)
        {
            if (trackFinished)
            {
                if (drawableTrack.CurrentTime == drawableTrack.Length)
                    SeekTo(0);

                trackFinished = false;
            }

            drawableTrack?.Start();
            framedClock.Start();

            if (isKeyboardAction)
                keyBindingAnimations.PlaySeekAnimation(KeyBindingAnimations.SeekAction.PlayPause, FontAwesome.Solid.Play);

            paused = false;
        }

        public void SetPlaybackSpeed(double speed)
        {
            playbackSpeed.Value = speed;
        }
    }
}
