// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using NekoPlayer.App.Config;
using NekoPlayer.App.Extensions;
using NekoPlayer.App.Localisation;
using NekoPlayer.App.Online;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osuTK.Graphics;

namespace NekoPlayer.App.Graphics.UserInterface
{
    public partial class ProfileImage : CompositeDrawable, IHasTooltip
    {
        private Sprite profileImage;

        private Google.Apis.YouTube.v3.Data.Channel channel;

        private LoadingSpinner loading;
        private Box hover;

        [Resolved]
        private TextureStore textureStore { get; set; }

        [Resolved]
        private YouTubeAPI api { get; set; }

        public virtual LocalisableString TooltipText { get; protected set; }

        [Resolved]
        private NekoPlayerAppBase app { get; set; }

        private Bindable<VideoMetadataTranslateSource> translationSource = new Bindable<VideoMetadataTranslateSource>();
        private Bindable<ProfileImageShape> profileImageShape;

        public ProfileImage(float size = 30)
        {
            Width = Height = size;
            //CornerRadius = size / 2;
            Masking = true;
            InternalChildren = new Drawable[]
            {
                samples,
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black
                },
                profileImage = new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                },
                hover = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White,
                    Blending = BlendingParameters.Additive,
                    Alpha = 0,
                },
                loading = new LoadingLayer(true, false, false)
            };
        }

        private Bindable<Localisation.Language> uiLanguage;

        [BackgroundDependencyLoader]
        private void load(ISampleStore tracks, OverlayColourProvider overlayColourProvider)
        {
            uiLanguage = app.CurrentLanguage.GetBoundCopy();
            BorderColour = overlayColourProvider.Light4;
            BorderThickness = 0;

            profileImageShape = appConfig.GetBindable<ProfileImageShape>(NekoPlayerSetting.ProfileImageShape);
            translationSource = appConfig.GetBindable<VideoMetadataTranslateSource>(NekoPlayerSetting.VideoMetadataTranslateSource);

            profileImageShape.BindValueChanged(shape =>
            {
                switch (shape.NewValue)
                {
                    case ProfileImageShape.Circle:
                        this.TransformTo(nameof(CornerRadius), Height / 2, 500, Easing.OutQuint);
                        break;

                    case ProfileImageShape.Square:
                        this.TransformTo(nameof(CornerRadius), NekoPlayerApp.UI_CORNER_RADIUS / 2, 500, Easing.OutQuint);
                        break;
                }
            }, true);
        }

        protected override bool OnHover(HoverEvent e)
        {
            hover.FadeTo(0.1f, 500, Easing.OutQuint);
            this.TransformTo(nameof(BorderThickness), 2f, 250, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            this.TransformTo(nameof(BorderThickness), 0f, 250, Easing.OutQuint);
            hover.FadeOut(500, Easing.OutQuint);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            profileImage.Dispose();
        }

        public void PlayClickAudio()
        {
            //clickAudio.Play();
        }

        private HoverSounds samples = new HoverClickSounds(HoverSampleSet.Default);

        [Resolved]
        private NekoPlayerConfigManager appConfig { get; set; }

        protected override bool OnClick(ClickEvent e)
        {
            PlayClickAudio();
            if (channel != null)
                app.Host.OpenUrlExternally($"https://www.youtube.com/channel/{channel.Id}");

            return base.OnClick(e);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            (samples as HoverClickSounds).Enabled.Value = true;
        }

        public void UpdateProfileImage(string channelId)
        {
            translationSource.UnbindEvents();
            uiLanguage.UnbindEvents();
            Task.Run(async () =>
            {
                channel = api.GetChannel(channelId);
                _ = Task.Run(async () =>
                {
                    await GetProfileImage(channel.Snippet.Thumbnails.High.Url);
                });
                TooltipText = NekoPlayerStrings.ProfileImageTooltip(api.GetLocalizedChannelTitle(channel, true), Convert.ToInt32(channel.Statistics.SubscriberCount).ToMetric(decimals: 2));

                translationSource.BindValueChanged(locale =>
                {
                    Task.Run(async () =>
                    {
                        TooltipText = NekoPlayerStrings.ProfileImageTooltip(api.GetLocalizedChannelTitle(channel, true), Convert.ToInt32(channel.Statistics.SubscriberCount).ToMetric(decimals: 2));
                    });
                }, true);

                uiLanguage.BindValueChanged(locale =>
                {
                    Task.Run(async () =>
                    {
                        TooltipText = NekoPlayerStrings.ProfileImageTooltip(api.GetLocalizedChannelTitle(channel, true), Convert.ToInt32(channel.Statistics.SubscriberCount).ToMetric(decimals: 2));
                    });
                }, true);
            });
        }

        public async Task GetProfileImage(string url, CancellationToken cancellationToken = default)
        {
            Schedule(() => loading.Show());
            Texture north = await textureStore.GetAsync(channel.Snippet.Thumbnails.High.Url, cancellationToken);
            Schedule(() => { profileImage.Texture = north; });
            Schedule(() => loading.Hide());
        }
    }
}
