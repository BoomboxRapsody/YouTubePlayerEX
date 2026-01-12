using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3.Data;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;
using YouTubePlayerEX.App.Config;
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

        [Resolved]
        private FrameworkConfigManager frameworkConfig { get; set; }

        [Resolved]
        private YTPlayerEXConfigManager appConfig { get; set; }

        private Bindable<string> localeBindable = new Bindable<string>();
        private Bindable<VideoMetadataTranslateSource> translationSource = new Bindable<VideoMetadataTranslateSource>();

        [BackgroundDependencyLoader]
        private void load()
        {
            localeBindable = frameworkConfig.GetBindable<string>(FrameworkSetting.Locale);
            translationSource = appConfig.GetBindable<VideoMetadataTranslateSource>(YTPlayerEXSetting.VideoMetadataTranslateSource);

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

        private Video videoData;

        public void UpdateVideo(string videoId)
        {
            Task.Run(async () =>
            {
                videoData = api.GetVideo(videoId);
                Channel channelData = api.GetChannel(videoData.Snippet.ChannelId);
                videoName.Text = api.GetLocalizedVideoTitle(videoData);
                desc.Text = YTPlayerEXStrings.VideoMetadataDesc(api.GetLocalizedChannelTitle(channelData), Convert.ToInt32(videoData.Statistics.ViewCount).ToStandardFormattedString(0));
                profileImage.UpdateProfileImage(videoData.Snippet.ChannelId);

                localeBindable.BindValueChanged(locale =>
                {
                    Task.Run(async () =>
                    {
                        videoName.Text = api.GetLocalizedVideoTitle(videoData);
                        desc.Text = YTPlayerEXStrings.VideoMetadataDesc(api.GetLocalizedChannelTitle(channelData), Convert.ToInt32(videoData.Statistics.ViewCount).ToStandardFormattedString(0));
                    });
                }, true);

                translationSource.BindValueChanged(locale =>
                {
                    Task.Run(async () =>
                    {
                        videoName.Text = api.GetLocalizedVideoTitle(videoData);
                        desc.Text = YTPlayerEXStrings.VideoMetadataDesc(api.GetLocalizedChannelTitle(channelData), Convert.ToInt32(videoData.Statistics.ViewCount).ToStandardFormattedString(0));
                    });
                }, true);
            });
        }
    }
}
