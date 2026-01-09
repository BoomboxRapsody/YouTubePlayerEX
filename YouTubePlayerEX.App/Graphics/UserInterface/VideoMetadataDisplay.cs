using Google.Apis.YouTube.v3.Data;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK.Graphics;

namespace YouTubePlayerEX.App.Graphics.UserInterface
{
    public partial class VideoMetadataDisplay : CompositeDrawable
    {
        private ProfileImage profileImage;
        private SpriteText videoName;

        [BackgroundDependencyLoader]
        private void load()
        {
            CornerRadius = 12;
            Masking = true;
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White,
                    Alpha = 0.1f,
                },
                new Container {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(7),
                    Children = new Drawable[]
                    {
                        profileImage = new ProfileImage(45)
                        {

                        },
                        videoName = new SpriteText
                        {
                            Text = "test test test 123 테스트"
                        }
                    }
                }
            };
        }

        public void UpdateVideo(Video videoData)
        {
            videoName.Text = videoData.Snippet.Title;
            profileImage.UpdateProfileImage(videoData.Snippet.ChannelId);
        }
    }
}
