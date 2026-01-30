// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3.Data;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using YouTubePlayerEX.App.Config;
using YouTubePlayerEX.App.Extensions;
using YouTubePlayerEX.App.Graphics.Sprites;
using YouTubePlayerEX.App.Localisation;
using YouTubePlayerEX.App.Online;

namespace YouTubePlayerEX.App.Graphics.UserInterface
{
    public partial class CommentDisplay : CompositeDrawable
    {
        private ProfileImage profileImage = null!;
        private TruncatingSpriteText channelName = null!;
        private TruncatingSpriteText commentText = null!;
        public Action<VideoMetadataDisplay> ClickEvent = null!;
        private AdaptiveSpriteText likeCount = null!, translateToText = null!;
        private RoundedButtonContainer translateButton = null!;

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
        private Bindable<VideoMetadataTranslateSource> translationSource = new Bindable<VideoMetadataTranslateSource>();

        public CommentDisplay(Comment comment)
        {
            commentData = comment;
            Height = 110;
            Task.Run(async () =>
            {
                UpdateData();
            });
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider overlayColourProvider)
        {
            localeBindable = frameworkConfig.GetBindable<string>(FrameworkSetting.Locale);
            translationSource = appConfig.GetBindable<VideoMetadataTranslateSource>(YTPlayerEXSetting.VideoMetadataTranslateSource);

            CornerRadius = 12;
            Masking = true;
            InternalChildren = new Drawable[]
            {
                samples,
                bgLayer = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = overlayColourProvider.Background4,
                    Alpha = 0.7f,
                },
                new Container {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(7),
                    Children = new Drawable[]
                    {
                        profileImage = new ProfileImage(35),
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding
                            {
                                Vertical = 5,
                                Left = 42,
                                Right = 5,
                            },
                            Children = new Drawable[]
                            {
                                channelName = new TruncatingSpriteText
                                {
                                    Font = YouTubePlayerEXApp.DefaultFont.With(size: 13, weight: "SemiBold"),
                                    RelativeSizeAxes = Axes.X,
                                    Text = "[channel name]",
                                    Colour = overlayColourProvider.Background1,
                                },
                                commentText = new TruncatingSpriteText
                                {
                                    Font = YouTubePlayerEXApp.DefaultFont.With(size: 17, weight: "Regular"),
                                    RelativeSizeAxes = Axes.X,
                                    Position = new Vector2(0, 13),
                                    Text = "[text]",
                                    Colour = overlayColourProvider.Content2,
                                },
                                translateButton = new RoundedButtonContainer
                                {
                                    AutoSizeAxes = Axes.X,
                                    Height = 27,
                                    CornerRadius = 12,
                                    Masking = true,
                                    AlwaysPresent = true,
                                    Position = new Vector2(0, 35),
                                    ClickAction = f => translateComment(),
                                    Children = new Drawable[]
                                    {
                                        new Container
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            CornerRadius = 12,
                                            Child = new Box
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Colour = overlayColourProvider.Background3,
                                                Alpha = 0.7f,
                                            },
                                        },
                                        new FillFlowContainer
                                        {
                                            AutoSizeAxes = Axes.X,
                                            RelativeSizeAxes = Axes.Y,
                                            Direction = FillDirection.Horizontal,
                                            Spacing = new Vector2(4, 0),
                                            Padding = new MarginPadding(8),
                                            Children = new Drawable[]
                                            {
                                                translateToText = new AdaptiveSpriteText
                                                {
                                                    Text = "[no metadata]",
                                                    Colour = overlayColourProvider.Content2,
                                                    Font = YouTubePlayerEXApp.DefaultFont.With(size: 12.5f, weight: "Regular"),
                                                },
                                            }
                                        }
                                    }
                                },
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Position = new Vector2(0, 65),
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(4, 0),
                                    Children = new Drawable[]
                                    {
                                        new Container
                                        {
                                            AutoSizeAxes = Axes.X,
                                            Height = 27,
                                            CornerRadius = 12,
                                            Masking = true,
                                            AlwaysPresent = true,
                                            Children = new Drawable[]
                                            {
                                                new Container
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    CornerRadius = 12,
                                                    Child = new Box
                                                    {
                                                        RelativeSizeAxes = Axes.Both,
                                                        Colour = overlayColourProvider.Background3,
                                                        Alpha = 0.7f,
                                                    },
                                                },
                                                new FillFlowContainer
                                                {
                                                    AutoSizeAxes = Axes.X,
                                                    RelativeSizeAxes = Axes.Y,
                                                    Direction = FillDirection.Horizontal,
                                                    Spacing = new Vector2(4, 0),
                                                    Padding = new MarginPadding(8),
                                                    Children = new Drawable[]
                                                    {
                                                        new SpriteIcon
                                                        {
                                                            Width = 12,
                                                            Height = 12,
                                                            Icon = FontAwesome.Solid.ThumbsUp,
                                                            Colour = overlayColourProvider.Content2,
                                                        },
                                                        likeCount = new AdaptiveSpriteText
                                                        {
                                                            Text = "[no metadata]",
                                                            Colour = overlayColourProvider.Content2,
                                                            Font = YouTubePlayerEXApp.DefaultFont.With(size: 12.5f, weight: "Regular"),
                                                        },
                                                    }
                                                }
                                            }
                                        },
                                    }
                                },
                            }
                        }
                    }
                }
            };
        }

        private bool translated;

        private Comment commentData, replyReference = null!;

        private HoverSounds samples = new HoverClickSounds(HoverSampleSet.Default);

        protected override void LoadComplete()
        {
            base.LoadComplete();
            if (samples is HoverClickSounds hoverClickSounds)
                hoverClickSounds.Enabled.Value = (ClickEvent != null);
        }

        private void translateComment()
        {
            if (translated == false)
            {
                Task.Run(async () => commentText.Text = translate.Translate(commentData.Snippet.TextOriginal, GoogleTranslateLanguage.auto));
                translateToText.Text = YTPlayerEXStrings.TranslateViewOriginal;
                translated = true;
            }
            else
            {
                commentText.Text = commentData.Snippet.TextOriginal;
                translateToText.Text = YTPlayerEXStrings.TranslateTo(app.CurrentLanguage.Value.GetLocalisableDescription());
                translated = false;
            }
        }

        [Resolved]
        private GoogleTranslate translate { get; set; } = null!;

        public void UpdateData()
        {
            Task.Run(async () =>
            {
                try
                {
                    DateTimeOffset? dateTime = commentData.Snippet.PublishedAtDateTimeOffset;
                    DateTimeOffset now = DateTime.Now;
                    Channel channelData = api.GetChannel(commentData.Snippet.AuthorChannelId.Value);
                    Channel? channelData2 = null;

                    if (replyReference != null)
                        channelData2 = api.GetChannel(replyReference.Snippet.AuthorChannelId.Value);

                    Schedule(() =>
                    {
                        channelName.Text = (channelData2 == null) ? api.GetLocalizedChannelTitle(channelData) : YTPlayerEXStrings.CommentReply(api.GetLocalizedChannelTitle(channelData), api.GetLocalizedChannelTitle(channelData2));
                        commentText.Text = commentData.Snippet.TextOriginal;
                        likeCount.Text = Convert.ToInt32(commentData.Snippet.LikeCount).ToStandardFormattedString(0);
                        translateToText.Text = YTPlayerEXStrings.TranslateTo(app.CurrentLanguage.Value.GetLocalisableDescription());
                        profileImage.UpdateProfileImage(commentData.Snippet.AuthorChannelId.Value);

                        localeBindable.BindValueChanged(locale =>
                        {
                            translateToText.Text = YTPlayerEXStrings.TranslateTo(app.CurrentLanguage.Value.GetLocalisableDescription());
                        }, true);
                    });
                }
                catch
                {
                    Hide();
                }
            });
        }
    }
}
