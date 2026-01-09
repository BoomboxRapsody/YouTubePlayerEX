using System.IO;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Video;
using osu.Framework.Timing;

namespace YouTubePlayerEX.App.Graphics.Videos
{
    public partial class YouTubeVideoPlayer : Container
    {
        private Video video;
        private Track track;

        private string fileName_Video, fileName_Audio;

        private StopwatchClock rateAdjustClock;
        private FramedClock framedClock;

        private Bindable<double> playbackSpeed;

        public YouTubeVideoPlayer(string fileName_Video, string fileName_Audio)
        {
            this.fileName_Video = fileName_Video;
            this.fileName_Audio = fileName_Audio;
        }

        [BackgroundDependencyLoader]
        private void load(ITrackStore tracks)
        {
            Task.Run(async () =>
            {
                track = tracks.GetFromStream(File.OpenRead(fileName_Audio), fileName_Audio);
                playbackSpeed = new Bindable<double>(1);

                rateAdjustClock = new StopwatchClock(true);
                framedClock = new FramedClock(rateAdjustClock);

                AddRangeInternal(new Drawable[] {
                    video = new Video(fileName_Video, false)
                    {
                        RelativeSizeAxes = osu.Framework.Graphics.Axes.Both,
                        Clock = framedClock,
                    }
                });

                rateAdjustClock.Rate = playbackSpeed.Value;

                track?.AddAdjustment(AdjustableProperty.Frequency, playbackSpeed);

                Play();
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
        }

        public void SeekTo(float pos)
        {
            video?.Seek(pos);
            track?.Seek(pos);
        }

        public void Pause()
        {
            track?.Stop();
            rateAdjustClock.Stop();
        }

        public void Play()
        {
            track?.Start();
            rateAdjustClock.Start();
        }

        public void SetPlaybackSpeed(double speed)
        {
            playbackSpeed.Value = speed;
        }
    }
}
