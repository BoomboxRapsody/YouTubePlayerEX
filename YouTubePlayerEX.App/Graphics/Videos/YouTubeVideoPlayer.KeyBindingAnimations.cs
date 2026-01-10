using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using YouTubePlayerEX.App.Graphics.Sprites;

namespace YouTubePlayerEX.App.Graphics.Videos
{
    public partial class YouTubeVideoPlayer
    {
        public partial class KeyBindingAnimations : Container
        {
            private SeekAnimation leftContent, rightContent;

            [BackgroundDependencyLoader]
            private void load()
            {
                AddRange(new Drawable[] {
                    leftContent = new SeekAnimation(SeekAction.FastRewind10sec) {
                        Anchor = Anchor.TopLeft,
                        Origin = Anchor.TopLeft,
                        RelativeSizeAxes = Axes.Y,
                        Width = 200,
                    },
                    rightContent = new SeekAnimation(SeekAction.FastForward10sec) {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        RelativeSizeAxes = Axes.Y,
                        Width = 200,
                    }
                });
            }

            public void PlaySeekAnimation(SeekAction seekAction)
            {
                switch (seekAction)
                {
                    case SeekAction.FastRewind10sec:
                    {
                        rightContent.HideNow();
                        leftContent.PlaySeekAnimation();
                        break;
                    }
                    case SeekAction.FastForward10sec:
                    {
                        leftContent.HideNow();
                        rightContent.PlaySeekAnimation();
                        break;
                    }
                }
            }

            private partial class SeekAnimation : Container
            {
                public bool IsVisible;
                private SeekAction trackAction;
                private SpriteIcon seekArrow;
                private AdaptiveSpriteText seekValue;

                private Container content;

                public SeekAnimation(SeekAction trackAction)
                {
                    this.trackAction = trackAction;
                    if (trackAction == SeekAction.FastRewind10sec)
                    {
                        Add(content = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Children = new Drawable[]
                                    {
                                        seekArrow = new SpriteIcon
                                        {
                                            Width = 25,
                                            Height = 25,
                                            Scale = new osuTK.Vector2(0.8f, 1),
                                            Position = new osuTK.Vector2(20, 0),
                                            Icon = FontAwesome.Solid.ChevronLeft,
                                        },
                                        seekValue = new AdaptiveSpriteText
                                        {
                                            Text = "- 10",
                                            Margin = new MarginPadding
                                            {
                                                Left = 30,
                                            },
                                            Font = YouTubePlayerEXApp.DefaultFont.With(size: 25),
                                        },
                                    }
                                }
                            }
                        });
                    }
                    else
                    {
                        Add(content = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Children = new Drawable[]
                                    {
                                        seekArrow = new SpriteIcon
                                        {
                                            Width = 25,
                                            Height = 25,
                                            Scale = new osuTK.Vector2(0.8f, 1),
                                            Position = new osuTK.Vector2(20, 0),
                                            Icon = FontAwesome.Solid.ChevronRight,
                                        },
                                        seekValue = new AdaptiveSpriteText
                                        {
                                            Text = "+ 10",
                                            Margin = new MarginPadding
                                            {
                                                Right = 45,
                                            },
                                            Font = YouTubePlayerEXApp.DefaultFont.With(size: 25),
                                        },
                                    }
                                }
                            }
                        });
                    }
                }

                [BackgroundDependencyLoader]
                private void load()
                {
                    content.FadeOut();
                }

                public void HideNow()
                {
                    content.FadeOut(250, Easing.In);
                    using (BeginDelayedSequence(250))
                    {
                        seekArrow.ScaleTo(new osuTK.Vector2(0.8f, 1));
                        if (trackAction == SeekAction.FastRewind10sec)
                        {
                            seekArrow.MoveTo(new osuTK.Vector2(20, 0));
                        }
                        else
                        {
                            seekArrow.MoveTo(new osuTK.Vector2(40, 0));
                        }
                    }
                }

                public void PlaySeekAnimation()
                {
                    content.FadeInFromZero(250, Easing.Out);
                    seekArrow.ScaleTo(new osuTK.Vector2(0.7f, 1));
                    if (trackAction == SeekAction.FastRewind10sec)
                    {
                        seekArrow.MoveTo(new osuTK.Vector2(20, 0));
                    }
                    else
                    {
                        seekArrow.MoveTo(new osuTK.Vector2(20, 0));
                    }
                    seekArrow.ScaleTo(1, 250, Easing.Out);
                    if (trackAction == SeekAction.FastRewind10sec)
                    {
                        seekArrow.MoveTo(new osuTK.Vector2(0), 500, Easing.OutQuart);
                    }
                    else
                    {
                        seekArrow.MoveTo(new osuTK.Vector2(40, 0), 500, Easing.OutQuart);
                    }
                    using (BeginDelayedSequence(1250))
                    {
                        HideNow();
                    }
                }
            }

            public enum SeekAction
            {
                FastForward10sec,
                FastRewind10sec,
            }
        }
    }
}
