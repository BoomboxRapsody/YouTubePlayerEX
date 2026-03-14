// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3.Data;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osuTK.Graphics;
using NekoPlayer.App.Config;
using NekoPlayer.App.Graphics.Sprites;
using NekoPlayer.App.Online;

namespace NekoPlayer.App.Graphics.UserInterface
{
    public partial class YouTubeChannelMetadataDisplay : CompositeDrawable
    {
        private ProfileImage profileImage;
        private TruncatingSpriteText videoName;
        private TruncatingSpriteText desc;
        public Action<YouTubeChannelMetadataDisplay> ClickEvent;

        private Box bgLayer, hover;

        [Resolved]
        private YouTubeAPI api { get; set; }

        [Resolved]
        private FrameworkConfigManager frameworkConfig { get; set; }

        [Resolved]
        private NekoPlayerConfigManager appConfig { get; set; }

        [Resolved]
        private NekoPlayerAppBase app { get; set; }

        private Bindable<Localisation.Language> uiLanguage;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider overlayColourProvider)
        {
            uiLanguage = app.CurrentLanguage.GetBoundCopy();

            CornerRadius = NekoPlayerApp.UI_CORNER_RADIUS;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                samples,
                bgLayer = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = overlayColourProvider.Background4,
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
                        profileImage = new ProfileImage(45),
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding
                            {
                                Vertical = 5,
                                Left = 50,
                                Right = 5,
                            },
                            Children = new Drawable[]
                            {
                                videoName = new TruncatingSpriteText
                                {
                                    Font = NekoPlayerApp.TorusAlternate.With(size: 20, weight: "Bold"),
                                    RelativeSizeAxes = Axes.X,
                                    Text = "",
                                    Colour = overlayColourProvider.Content2,
                                },
                                desc = new TruncatingSpriteText
                                {
                                    Font = NekoPlayerApp.DefaultFont.With(size: 13, weight: "SemiBold"),
                                    RelativeSizeAxes = Axes.X,
                                    Colour = overlayColourProvider.Foreground2,
                                    Text = "",
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

        public void UpdateUser(Channel channel)
        {
            uiLanguage.UnbindEvents();
            Task.Run(async () =>
            {
                Schedule(() =>
                {
                    videoName.Text = api.GetLocalizedChannelTitle(channel);
                    desc.Text = channel.Snippet.CustomUrl;
                    profileImage.UpdateProfileImage(channel.Id);
                });
            });

            uiLanguage.BindValueChanged(locale =>
            {
                Task.Run(async () =>
                {
                    Schedule(() =>
                    {
                        videoName.Text = api.GetLocalizedChannelTitle(channel);
                    });
                });
            });
        }
    }
}
