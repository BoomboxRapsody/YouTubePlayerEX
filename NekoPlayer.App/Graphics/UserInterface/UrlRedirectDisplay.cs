// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NekoPlayer.App.Config;
using NekoPlayer.App.Graphics.Sprites;
using NekoPlayer.App.Online;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osuTK.Graphics;
using YoutubeExplode.Videos;

namespace NekoPlayer.App.Graphics.UserInterface
{
    public partial class UrlRedirectDisplay : AdaptiveClickableContainer
    {
        private string url;

        private AdaptiveSpriteText displayName;

        protected Box Hover;

        public UrlRedirectDisplay(string url)
            : base(HoverSampleSet.Button)
        {
            this.url = url;
            Enabled.Value = true;
            Masking = true;
            TooltipText = url;
        }

        private SpriteIcon icon;
        private Bindable<Localisation.Language> uiLanguage = null!;
        private Bindable<UsernameDisplayMode> usernameDisplayMode = null!;

        [Resolved]
        private NekoPlayerConfigManager appConfig { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider overlayColourProvider)
        {
            uiLanguage = app.CurrentLanguage.GetBoundCopy();
            usernameDisplayMode = appConfig.GetBindable<UsernameDisplayMode>(NekoPlayerSetting.UsernameDisplayMode);
            AutoSizeAxes = Axes.Both;

            AddRangeInternal(new Drawable[]
            {
                new CircularContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = overlayColourProvider.Background2,
                        },
                        new FillFlowContainer
                        {
                            Margin = new MarginPadding(2),
                            AutoSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                icon = new SpriteIcon
                                {
                                    Size = new osuTK.Vector2(12),
                                    Margin = new MarginPadding(4),
                                },
                                displayName = new AdaptiveSpriteText
                                {
                                    Margin = new MarginPadding(2),
                                    Text = url,
                                }
                            }
                        },
                        Hover = new Box
                        {
                            Alpha = 0,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.White,
                            Blending = BlendingParameters.Additive,
                            Depth = float.MinValue
                        },
                    }
                }
            });

            if (NekoPlayerDescriptionParser.IsYouTubeVideo(url))
            {
                icon.Icon = FontAwesome.Brands.Youtube;

                string videoId = VideoId.Parse(url);
                Google.Apis.YouTube.v3.Data.Video video = api.GetVideo(videoId);

                displayName.Text = api.GetLocalizedVideoTitle(video);

                uiLanguage.BindValueChanged(locale =>
                {
                    Schedule(() =>
                    {
                        displayName.Text = api.GetLocalizedVideoTitle(video);
                    });
                });
            }
            else if (NekoPlayerDescriptionParser.IsYouTubeChannel(url))
            {
                icon.Icon = FontAwesome.Brands.Youtube;

                string channelId = url.Replace("https://www.youtube.com/channel/", string.Empty);
                Google.Apis.YouTube.v3.Data.Channel channel = api.GetChannel(channelId);

                displayName.Text = api.GetLocalizedChannelTitle(channel);

                uiLanguage.BindValueChanged(locale =>
                {
                    Schedule(() =>
                    {
                        displayName.Text = api.GetLocalizedChannelTitle(channel);
                    });
                });

                usernameDisplayMode.BindValueChanged(locale =>
                {
                    Schedule(() =>
                    {
                        displayName.Text = api.GetLocalizedChannelTitle(channel);
                    });
                });
            }
            else if (NekoPlayerDescriptionParser.IsDiscord(url))
            {
                icon.Icon = FontAwesome.Brands.Discord;
                displayName.Text = "discord";
            }
            else if (NekoPlayerDescriptionParser.IsTwitch(url))
            {
                icon.Icon = FontAwesome.Brands.Twitch;
                displayName.Text = url.Replace("https://www.twitch.tv/", string.Empty).Replace("https://twitch.tv/", string.Empty);
            }
            else if (NekoPlayerDescriptionParser.IsTwitter(url))
            {
                icon.Icon = FontAwesome.Brands.Twitter;
                displayName.Text = url.Replace("https://x.com/", string.Empty).Replace("https://twitter.com/", string.Empty);
            }
        }

        [Resolved]
        private GameHost host { get; set; }

        [Resolved]
        private YouTubeAPI api { get; set; }

        [Resolved]
        private NekoPlayerAppBase app { get; set; }

        protected virtual float HoverLayerFinalAlpha => 0.1f;

        protected override bool OnHover(HoverEvent e)
        {
            if (Enabled.Value)
            {
                Hover.FadeTo(0.2f, 40, Easing.OutQuint)
                     .Then()
                     .FadeTo(HoverLayerFinalAlpha, 800, Easing.OutQuint);
            }

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);

            Hover.FadeOut(800, Easing.OutQuint);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (!NekoPlayerDescriptionParser.IsYouTubeVideo(url))
                host.OpenUrlExternally(url);
            else
                app.AppMessageHandler.SelectVideo(url);

            return base.OnClick(e);
        }
    }
}
