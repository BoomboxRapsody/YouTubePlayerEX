using Google.Apis.YouTube.v3.Data;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK.Graphics;
using YouTubePlayerEX.App.Online;

namespace YouTubePlayerEX.App.Graphics.UserInterface
{
    public partial class ProfileImage : CompositeDrawable
    {
        private Sprite profileImage;

        [Resolved]
        private TextureStore textureStore { get; set; }

        [Resolved]
        private YouTubeAPI api { get; set; }

        public ProfileImage(float size = 30)
        {
            Width = Height = size;
            CornerRadius = size / 2;
            Masking = true;
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black
                },
                profileImage = new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                }
            };
        }

        public void UpdateProfileImage(string channelId)
        {
            Channel channel = api.GetChannel(channelId);
            profileImage.Texture = textureStore.Get(channel.Snippet.Thumbnails.High.Url);
        }
    }
}
