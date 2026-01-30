// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Framework.Graphics.Containers;

namespace YouTubePlayerEX.App.Graphics.Cursor
{
    public partial class AdaptiveTooltipContainer : TooltipContainer
    {
        protected override ITooltip CreateTooltip() => new OsuTooltip();

        public AdaptiveTooltipContainer(CursorContainer cursor)
            : base(cursor)
        {
        }

        protected override double AppearDelay => (1 - CurrentTooltip.Alpha) * base.AppearDelay; // reduce appear delay if the tooltip is already partly visible.

        public partial class OsuTooltip : Tooltip
        {
            private const float max_width = 500;

            private readonly Box background;
            private readonly TextFlowContainer text;
            private bool instantMovement = true;

            private LocalisableString lastContent;

            public override void SetContent(LocalisableString content)
            {
                if (content.Equals(lastContent))
                    return;

                text.Text = content;

                if (IsPresent)
                {
                    AutoSizeDuration = 250;
                    background.FlashColour(new Color4(0.4f, 0.4f, 0.4f, 1f), 1000, Easing.OutQuint);
                }
                else
                    AutoSizeDuration = 0;

                lastContent = content;
            }

            public OsuTooltip()
            {
                AutoSizeEasing = Easing.OutQuint;

                CornerRadius = 5;
                Masking = true;
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Colour = Color4.Black.Opacity(40),
                    Radius = 5,
                };
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.9f,
                    },
                    text = new TextFlowContainer(f =>
                    {
                        f.Font = YouTubePlayerEXApp.DefaultFont;
                    })
                    {
                        Margin = new MarginPadding(5),
                        AutoSizeAxes = Axes.Both,
                        MaximumSize = new Vector2(max_width, float.PositiveInfinity),
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                background.Colour = Color4Extensions.FromHex(@"333");
            }

            protected override void PopIn()
            {
                instantMovement |= !IsPresent;
                this.FadeIn(500, Easing.OutQuint);
            }

            protected override void PopOut() => this.Delay(150).FadeOut(500, Easing.OutQuint);

            public override void Move(Vector2 pos)
            {
                if (instantMovement)
                {
                    Position = pos;
                    instantMovement = false;
                }
                else
                {
                    this.MoveTo(pos, 200, Easing.OutQuint);
                }
            }
        }
    }
}
