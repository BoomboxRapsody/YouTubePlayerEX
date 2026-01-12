using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osuTK.Graphics;
using YouTubePlayerEX.App.Config;
using YouTubePlayerEX.App.Extensions;
using YouTubePlayerEX.App.Localisation;
using YouTubePlayerEX.App.Online;

namespace YouTubePlayerEX.App.Graphics.UserInterface
{
    public partial class ProfileImage : CompositeDrawable, IHasTooltip
    {
        private Sprite profileImage;

        private Google.Apis.YouTube.v3.Data.Channel channel;

        [Resolved]
        private TextureStore textureStore { get; set; }

        [Resolved]
        private YouTubeAPI api { get; set; }

        public virtual LocalisableString TooltipText { get; protected set; }

        [Resolved]
        private YouTubePlayerEXAppBase app { get; set; }

        private Bindable<VideoMetadataTranslateSource> translationSource = new Bindable<VideoMetadataTranslateSource>();

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

        private Sample clickAudio;

        [BackgroundDependencyLoader]
        private void load(ISampleStore tracks)
        {
            clickAudio = tracks.Get("button-select.wav");
        }

        public void PlayClickAudio()
        {
            clickAudio.Play();
        }

        [Resolved]
        private YTPlayerEXConfigManager appConfig { get; set; }

        protected override bool OnClick(ClickEvent e)
        {
            PlayClickAudio();
            if (channel != null)
                app.Host.OpenUrlExternally($"https://www.youtube.com/channel/{channel.Id}");

            return base.OnClick(e);
        }

        public void UpdateProfileImage(string channelId)
        {
            Task.Run(async () =>
            {
                channel = api.GetChannel(channelId);
                profileImage.Texture = textureStore.Get(channel.Snippet.Thumbnails.High.Url);
                TooltipText = YTPlayerEXStrings.ProfileImageTooltip(api.GetLocalizedChannelTitle(channel), Convert.ToInt32(channel.Statistics.SubscriberCount).ToStandardFormattedString(0));

                translationSource.BindValueChanged(locale =>
                {
                    Task.Run(async () =>
                    {
                        TooltipText = YTPlayerEXStrings.ProfileImageTooltip(api.GetLocalizedChannelTitle(channel), Convert.ToInt32(channel.Statistics.SubscriberCount).ToStandardFormattedString(0));
                    });
                }, true);
            });
        }
    }
}
