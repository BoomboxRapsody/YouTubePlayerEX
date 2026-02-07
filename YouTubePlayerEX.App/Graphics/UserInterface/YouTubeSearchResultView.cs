// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3.Data;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
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
    public partial class YouTubeSearchResultView : AdaptiveClickableContainer
    {
        private Sprite thumbnail = null!;
        private AdaptiveTextFlowContainer videoNameText = null!;
        private TruncatingSpriteText channelNameText = null!;
        public Action<VideoMetadataDisplay> ClickEvent = null!;
        private AdaptiveSpriteText viewsText = null!;

        private Box bgLayer = null!;

        [Resolved]
        private YouTubeAPI api { get; set; } = null!;

        [Resolved]
        private YouTubePlayerEXAppBase app { get; set; } = null!;

        [Resolved]
        private FrameworkConfigManager frameworkConfig { get; set; } = null!;

        [Resolved]
        private YTPlayerEXConfigManager appConfig { get; set; } = null!;

        private Bindable<string> localeBindable = new Bindable<string>();

        public YouTubeSearchResultView()
        {
            Height = 110;
        }

        [Resolved]
        private TextureStore textureStore { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider overlayColourProvider)
        {
            localeBindable = frameworkConfig.GetBindable<string>(FrameworkSetting.Locale);

            CornerRadius = 12;
            Masking = true;
            Children = new Drawable[]
            {
                samples,
                bgLayer = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = overlayColourProvider.Background4,
                    Alpha = 0.7f,
                },
                Hover = new Box
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
                            Width = 190,
                            Height = 96,
                            Masking = true,
                            CornerRadius = 12,
                            Children = new Drawable[] {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.Black,
                                },
                                thumbnail = new Sprite
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    FillMode = FillMode.Stretch,
                                },
                                loading = new LoadingLayer(true, false, true)
                            },
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding
                            {
                                Top = 25,
                                Bottom = 5,
                                Left = 200,
                                Right = 5,
                            },
                            Children = new Drawable[]
                            {
                                videoNameText = new AdaptiveTextFlowContainer(f => f.Font = YouTubePlayerEXApp.DefaultFont.With(size: 17, weight: "Regular"))
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Text = "[text]",
                                    Colour = overlayColourProvider.Content2,
                                },
                                channelNameText = new TruncatingSpriteText
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Position = new Vector2(0, 17),
                                    Text = "[channel name]",
                                    Font = YouTubePlayerEXApp.DefaultFont.With(size: 13, weight: "SemiBold"),
                                    Colour = overlayColourProvider.Background1,
                                },
                                viewsText = new TruncatingSpriteText
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Position = new Vector2(0, (17 + 13)),
                                    Text = "0 views • 1 year ago",
                                    Font = YouTubePlayerEXApp.DefaultFont.With(size: 13, weight: "SemiBold"),
                                    Colour = overlayColourProvider.Background1,
                                },
                            }
                        }
                    }
                }
            };
        }

        public SearchResult Data = null!;

        private HoverSounds samples = new HoverClickSounds(HoverSampleSet.Default);

        protected override void LoadComplete()
        {
            base.LoadComplete();
            if (samples is HoverClickSounds hoverClickSounds)
                hoverClickSounds.Enabled.Value = (ClickEvent != null);
        }

        public async Task GetThumbnail(string url, CancellationToken cancellationToken = default)
        {
            Schedule(() => loading.Show());
            Texture north = await textureStore.GetAsync(Data.Snippet.Thumbnails.High.Url, cancellationToken);
            Schedule(() => { thumbnail.Texture = north; });
            Schedule(() => loading.Hide());
        }

        private LoadingSpinner loading = null!;

        protected override bool OnHover(HoverEvent e)
        {
            Hover.FadeTo(0.1f, 500, Easing.OutQuint);

            return base.OnHover(e);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            Content.FadeColour(Color4.Gray, 2000, Easing.OutQuint);
            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            Content.FadeColour(Color4.White, 1000, Easing.OutQuint);
            base.OnMouseUp(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);

            Hover.FadeOut(500, Easing.OutQuint);
        }

        protected Box Hover = null!;

        public Color4 BackgroundColour { get; set; }

        private Color4? flashColour;

        /// <summary>
        /// The colour the background will flash with when this button is clicked.
        /// </summary>
        public Color4 FlashColour
        {
            get => flashColour ?? BackgroundColour;
            set => flashColour = value;
        }

        private void trigger()
        {
            if (Enabled.Value)
                ClickAction?.Invoke(this);
        }

        public Action<YouTubeSearchResultView>? ClickAction { get; set; }

        protected override bool OnClick(ClickEvent e)
        {
            if (!IsHovered)
                Hover.FadeOutFromOne(1600);

            Hover.FlashColour(FlashColour, 800, Easing.OutQuint);
            trigger();

            return base.OnClick(e);
        }

        public void UpdateData()
        {
            Task.Run(async () =>
            {
                try
                {
                    DateTimeOffset? dateTime = Data.Snippet.PublishedAtDateTimeOffset;
                    DateTime now = DateTime.Now;
                    Channel channelData = api.GetChannel(Data.Snippet.ChannelId);
                    Video videoData = api.GetVideo(Data.Id.VideoId);

                    Schedule(() =>
                    {
                        channelNameText.Text = api.GetLocalizedChannelTitle(channelData, true);
                        videoNameText.Text = api.GetLocalizedVideoTitle(videoData);
#pragma warning disable CS8629 // Nullable 값 형식이 null일 수 있습니다.
                        viewsText.Text = YTPlayerEXStrings.VideoMetadataDescWithoutChannelName(Convert.ToInt32(videoData.Statistics.ViewCount).ToStandardFormattedString(0), dateTime.Value.DateTime.Humanize(dateToCompareAgainst: now));
#pragma warning restore CS8629 // Nullable 값 형식이 null일 수 있습니다.

                        localeBindable.BindValueChanged(locale =>
                        {
                            channelNameText.Text = api.GetLocalizedChannelTitle(channelData, true);
                            videoNameText.Text = api.GetLocalizedVideoTitle(videoData);
                            viewsText.Text = YTPlayerEXStrings.VideoMetadataDescWithoutChannelName(Convert.ToInt32(videoData.Statistics.ViewCount).ToStandardFormattedString(0), dateTime.Value.DateTime.Humanize(dateToCompareAgainst: now));
                        }, true);
                    });

                    await GetThumbnail(Data.Snippet.Thumbnails.High.Url);
                }
                catch
                {
                    Hide();
                }
            });
        }
    }
}
