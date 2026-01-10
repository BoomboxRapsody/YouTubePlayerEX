using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3.Data;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;
using YouTubePlayerEX.App.Extensions;
using YouTubePlayerEX.App.Graphics.Sprites;
using YouTubePlayerEX.App.Localisation;
using YouTubePlayerEX.App.Online;

namespace YouTubePlayerEX.App.Graphics.UserInterface
{
    public partial class VideoMetadataDisplay : CompositeDrawable
    {
        private ProfileImage profileImage;
        private TruncatingSpriteText videoName;
        private TruncatingSpriteText desc;

        [Resolved]
        private YouTubeAPI api { get; set; }

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
                        profileImage = new ProfileImage(45),
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding
                            {
                                Vertical = 5,
                                Left = 50,
                            },
                            Children = new Drawable[]
                            {
                                videoName = new TruncatingSpriteText
                                {
                                    Font = YouTubePlayerEXApp.DefaultFont.With(size: 20, weight: "Bold"),
                                    RelativeSizeAxes = Axes.X,
                                    Text = "please enter a video id!",
                                },
                                desc = new TruncatingSpriteText
                                {
                                    Font = YouTubePlayerEXApp.DefaultFont.With(size: 13, weight: "Regular"),
                                    RelativeSizeAxes = Axes.X,
                                    Colour = Color4.Gray,
                                    Text = "[no metadata available]",
                                    Position = new osuTK.Vector2(0, 20),
                                }
                            }
                        }
                    }
                }
            };
        }

        public void UpdateVideo(string videoId)
        {
            Task.Run(async () =>
            {
                Video videoData = api.GetVideo(videoId);
                Channel channelData = api.GetChannel(videoData.Snippet.ChannelId);
                videoName.Text = api.GetLocalizedVideoTitle(videoData);
                desc.Text = YTPlayerEXStrings.VideoMetadataDesc(api.GetLocalizedChannelTitle(channelData), Convert.ToInt32(videoData.Statistics.ViewCount).ToStandardFormattedString(0));
                profileImage.UpdateProfileImage(videoData.Snippet.ChannelId);
            });
        }
    }
}
