using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Video;

namespace YouTubePlayerEX.App.Graphics.Videos
{
    public partial class YouTubeVideoPlayer : CompositeDrawable
    {
        private Video video;

        public YouTubeVideoPlayer(string fileName)
        {
            InternalChild = video = new Video(fileName)
            {
                RelativeSizeAxes = osu.Framework.Graphics.Axes.Both,
            };
        }

        public void SetSource()
        {

        }
    }
}
