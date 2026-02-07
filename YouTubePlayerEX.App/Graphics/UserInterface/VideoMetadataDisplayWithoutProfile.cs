// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3.Data;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Graphics;
using YouTubePlayerEX.App.Config;
using YouTubePlayerEX.App.Extensions;
using YouTubePlayerEX.App.Graphics.Sprites;
using YouTubePlayerEX.App.Localisation;
using YouTubePlayerEX.App.Online;

namespace YouTubePlayerEX.App.Graphics.UserInterface
{
    public partial class VideoMetadataDisplayWithoutProfile : CompositeDrawable
    {
        private TruncatingSpriteText videoName;
        private TruncatingSpriteText desc;
        public Action<VideoMetadataDisplayWithoutProfile> ClickEvent;

        private Box bgLayer, hover;

        [Resolved]
        private YouTubeAPI api { get; set; }

        [Resolved]
        private FrameworkConfigManager frameworkConfig { get; set; }

        [Resolved]
        private YTPlayerEXConfigManager appConfig { get; set; }

        private Bindable<string> localeBindable = new Bindable<string>();
        private Bindable<UsernameDisplayMode> usernameDisplayMode;
        private Bindable<VideoMetadataTranslateSource> translationSource = new Bindable<VideoMetadataTranslateSource>();

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider overlayColourProvider)
        {
            localeBindable = frameworkConfig.GetBindable<string>(FrameworkSetting.Locale);
            usernameDisplayMode = appConfig.GetBindable<UsernameDisplayMode>(YTPlayerEXSetting.UsernameDisplayMode);
            translationSource = appConfig.GetBindable<VideoMetadataTranslateSource>(YTPlayerEXSetting.VideoMetadataTranslateSource);

            CornerRadius = YouTubePlayerEXApp.UI_CORNER_RADIUS;
            Masking = true;

            EdgeEffect = new osu.Framework.Graphics.Effects.EdgeEffectParameters
            {
                Type = osu.Framework.Graphics.Effects.EdgeEffectType.Shadow,
                Colour = Color4.Black.Opacity(0.25f),
                Offset = new Vector2(0, 2),
                Radius = 16,
            };

            InternalChildren = new Drawable[]
            {
                samples,
                bgLayer = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = overlayColourProvider.Background5,
                    Alpha = 1f,
                },
                hover = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.White,
                    Blending = BlendingParameters.Additive,
                    Alpha = 0,
                },
                new Container {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(7),
                    Children = new Drawable[]
                    {   
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding
                            {
                                Vertical = 5,
                                Horizontal = 5,
                            },
                            Children = new Drawable[]
                            {
                                videoName = new TruncatingSpriteText
                                {
                                    Font = YouTubePlayerEXApp.DefaultFont.With(family: "Torus-Alternate", size: 20, weight: "Bold"),
                                    RelativeSizeAxes = Axes.X,
                                    Text = "please enter a video id!",
                                    Colour = overlayColourProvider.Content2,
                                },
                                desc = new TruncatingSpriteText
                                {
                                    Font = YouTubePlayerEXApp.DefaultFont.With(size: 13, weight: "SemiBold"),
                                    RelativeSizeAxes = Axes.X,
                                    Text = "[no metadata available]",
                                    Colour = overlayColourProvider.Background1,
                                    Position = new osuTK.Vector2(0, 20),
                                }
                            }
                        }
                    }
                }
            };
        }

        private Video videoData;

        protected override bool OnClick(ClickEvent e)
        {
            ClickEvent?.Invoke(this);

            return base.OnClick(e);
        }

        private HoverSounds samples = new HoverClickSounds(HoverSampleSet.Default);

        protected override bool OnHover(HoverEvent e)
        {
            if (ClickEvent != null)
                hover.FadeTo(0.1f, 500, Easing.OutQuint);

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);

            if (ClickEvent != null)
                hover.FadeOut(500, Easing.OutQuint);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            (samples as HoverClickSounds).Enabled.Value = (ClickEvent != null);
        }

        public void UpdateVideo(string videoId)
        {
            Task.Run(async () =>
            {
                videoData = api.GetVideo(videoId);
                DateTimeOffset? dateTime = videoData.Snippet.PublishedAtDateTimeOffset;
                DateTimeOffset now = DateTimeOffset.Now;
                Channel channelData = api.GetChannel(videoData.Snippet.ChannelId);
                videoName.Text = api.GetLocalizedVideoTitle(videoData);
                desc.Text = YTPlayerEXStrings.VideoMetadataDesc(api.GetLocalizedChannelTitle(channelData), Convert.ToInt32(videoData.Statistics.ViewCount).ToStandardFormattedString(0), dateTime.Value.Humanize(dateToCompareAgainst: now));

                localeBindable.BindValueChanged(locale =>
                {
                    Task.Run(async () =>
                    {
                        videoName.Text = api.GetLocalizedVideoTitle(videoData);
                        desc.Text = YTPlayerEXStrings.VideoMetadataDesc(api.GetLocalizedChannelTitle(channelData), Convert.ToInt32(videoData.Statistics.ViewCount).ToStandardFormattedString(0), dateTime.Value.Humanize(dateToCompareAgainst: now));
                    });
                });

                usernameDisplayMode.BindValueChanged(locale =>
                {
                    Task.Run(async () =>
                    {
                        desc.Text = YTPlayerEXStrings.VideoMetadataDesc(api.GetLocalizedChannelTitle(channelData), Convert.ToInt32(videoData.Statistics.ViewCount).ToStandardFormattedString(0), dateTime.Value.Humanize(dateToCompareAgainst: now));
                    });
                }, true);

                translationSource.BindValueChanged(locale =>
                {
                    Task.Run(async () =>
                    {
                        videoName.Text = api.GetLocalizedVideoTitle(videoData);
                        desc.Text = YTPlayerEXStrings.VideoMetadataDesc(api.GetLocalizedChannelTitle(channelData), Convert.ToInt32(videoData.Statistics.ViewCount).ToStandardFormattedString(0), dateTime.Value.Humanize(dateToCompareAgainst: now));
                    });
                }, true);
            });
        }
    }
}
