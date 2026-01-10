using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Video;
using osu.Framework.Timing;
using YoutubeExplode.Videos.ClosedCaptions;
using YouTubePlayerEX.App.Graphics.Caption;
using YouTubePlayerEX.App.Online;

namespace YouTubePlayerEX.App.Graphics.Videos
{
    public partial class YouTubeVideoPlayer : Container
    {
        private Video video;
        private Track track;
        private DrawableTrack drawableTrack;

        private string fileName_Video, fileName_Audio;
        private ClosedCaptionTrack captionTrack;

        private StopwatchClock rateAdjustClock;
        private FramedClock framedClock;

        private Bindable<double> playbackSpeed;

        public YouTubeVideoPlayer(string fileName_Video, string fileName_Audio, ClosedCaptionTrack captionTrack)
        {
            this.fileName_Video = fileName_Video;
            this.fileName_Audio = fileName_Audio;
            this.captionTrack = captionTrack;
        }

        public BindableNumber<double> VideoProgress = new BindableNumber<double>()
        {
            MinValue = 0,
            MaxValue = 1,
        };

        private KeyBindingAnimations keyBindingAnimations;

        [BackgroundDependencyLoader]
        private void load(ITrackStore tracks)
        {
            track = tracks.GetFromStream(File.OpenRead(fileName_Audio), fileName_Audio);
            playbackSpeed = new Bindable<double>(1);

            rateAdjustClock = new StopwatchClock(true);
            framedClock = new FramedClock(rateAdjustClock);

            AddRange(new Drawable[] {
                drawableTrack = new DrawableTrack(track),
                video = new Video(fileName_Video, false)
                {
                    RelativeSizeAxes = osu.Framework.Graphics.Axes.Both,
                    FillMode = FillMode.Fit,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Clock = framedClock,
                },
                keyBindingAnimations = new KeyBindingAnimations
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new ClosedCaptionContainer(this, captionTrack)
            });

            rateAdjustClock.Rate = playbackSpeed.Value;

            playbackSpeed.BindValueChanged(v =>
            {
                rateAdjustClock.Rate = v.NewValue;
            });

            drawableTrack?.AddAdjustment(AdjustableProperty.Frequency, playbackSpeed);

            drawableTrack.Completed += trackCompleted;

            Play();
        }

        private void trackCompleted()
        {
            Pause();
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
        }

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
            video?.Seek(pos);
            drawableTrack?.Seek(pos);
        }

        public void FastForward10Sec()
        {
            video?.Seek(drawableTrack.CurrentTime + 10000);
            drawableTrack?.Seek(drawableTrack.CurrentTime + 10000);
            keyBindingAnimations.PlaySeekAnimation(KeyBindingAnimations.SeekAction.FastForward10sec);
        }

        public void FastRewind10Sec()
        {
            video?.Seek(drawableTrack.CurrentTime - 10000);
            drawableTrack?.Seek(drawableTrack.CurrentTime - 10000);
            keyBindingAnimations.PlaySeekAnimation(KeyBindingAnimations.SeekAction.FastRewind10sec);
        }

        public void Pause()
        {
            drawableTrack?.Stop();
            rateAdjustClock.Stop();
        }

        public void Play()
        {
            drawableTrack?.Start();
            rateAdjustClock.Start();
        }

        public void SetPlaybackSpeed(double speed)
        {
            playbackSpeed.Value = speed;
        }
    }
}
