using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Video;

namespace YouTubePlayerEX.App.Graphics.Videos
{
    public partial class YouTubeVideoPlayer : Container
    {
        private Video video;
        private ITrack track;

        private string fileName_Video, fileName_Audio;

        public YouTubeVideoPlayer(string fileName_Video, string fileName_Audio)
        {
            this.fileName_Video = fileName_Video;
            this.fileName_Audio = fileName_Audio;
        }

        [BackgroundDependencyLoader]
        private void load(ITrackStore tracks)
        {
            track = tracks.Get(fileName_Audio);

            AddRange(new Drawable[] {
                video = new Video(fileName_Video)
                {
                    RelativeSizeAxes = osu.Framework.Graphics.Axes.Both,
                }
            });
        }
    }
}
