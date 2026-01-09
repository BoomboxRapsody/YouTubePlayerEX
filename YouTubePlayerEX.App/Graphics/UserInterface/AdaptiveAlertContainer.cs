using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osuTK;
using osuTK.Graphics;

namespace YouTubePlayerEX.App.Graphics.UserInterface
{
    public partial class AdaptiveAlertContainer : VisibilityContainer
    {
        public LocalisableString Text
        {
            get => SpriteText.Text;
            set => SpriteText.Text = value;
        }

        protected SpriteText SpriteText;

        protected virtual float CaretWidth => 2;

        private const float caret_move_time = 60;

        protected virtual Color4 SelectionColour => FrameworkColour.YellowGreen;

        protected Color4 BackgroundCommit { get; set; } = FrameworkColour.Green;

        private Color4 backgroundFocused = new Color4(100, 100, 100, 255);
        private Color4 backgroundUnfocused = new Color4(100, 100, 100, 120);

        private readonly Box background;

        private readonly Container content;

        protected Color4 BackgroundFocused
        {
            get => backgroundFocused;
            set
            {
                backgroundFocused = value;
                if (HasFocus)
                    background.Colour = value;
            }
        }

        protected Color4 BackgroundUnfocused
        {
            get => backgroundUnfocused;
            set
            {
                backgroundUnfocused = value;
                if (!HasFocus)
                    background.Colour = value;
            }
        }

        protected virtual Color4 InputErrorColour => Color4.Red;

        public AdaptiveAlertContainer()
        {
            base.Content.Add(content = new Container
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                CornerRadius = 5,
                Masking = true,
                EdgeEffect = new EdgeEffectParameters
                {
                    Colour = Color4.White.Opacity(0.1f),
                    Type = EdgeEffectType.Shadow,
                    Radius = 5,
                },
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Transparent,
                        Alpha = 0,
                    },
                    SpriteText = CreateText()
                }
            });

            BackgroundFocused = FrameworkColour.BlueGreen;
            BackgroundUnfocused = FrameworkColour.BlueGreenDark;
        }

        protected virtual SpriteText CreateText() => new SpriteText
        {
            Depth = -1,
            Origin = Anchor.Centre,
            Anchor = Anchor.Centre,
            Font = YouTubePlayerEXApp.DefaultFont,
            Colour = FrameworkColour.Yellow
        };

        protected override void OnFocusLost(FocusLostEvent e)
        {
            base.OnFocusLost(e);

            background.ClearTransforms();
            background.Colour = BackgroundFocused;
            background.FadeColour(BackgroundUnfocused, 200, Easing.OutExpo);
        }

        protected override void OnFocus(FocusEvent e)
        {
            base.OnFocus(e);

            background.ClearTransforms();
            background.Colour = BackgroundUnfocused;
            background.FadeColour(BackgroundFocused, 200, Easing.Out);
        }

        public const float TRANSITION_DURATION = 500;

        protected override void PopIn()
        {
            content.ScaleTo(1, TRANSITION_DURATION * 2, Easing.OutElastic);
            this.FadeIn(TRANSITION_DURATION * 2, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            content.ScaleTo(0.8f, TRANSITION_DURATION / 2, Easing.InQuart);
            this.FadeOut(TRANSITION_DURATION, Easing.OutQuint);
        }

        public partial class FallingDownContainer : Container
        {
            public override void Show()
            {
                var col = (Color4)Colour;
                this.FadeColour(col.Opacity(0)).FadeColour(col, caret_move_time * 2, Easing.Out);
            }

            public override void Hide()
            {
                this.FadeOut(200);
                this.MoveToY(DrawSize.Y, 200, Easing.InQuad);
            }
        }

        public partial class FadingPlaceholderText : SpriteText
        {
            public override void Show() => this.FadeIn(200);

            public override void Hide() => this.FadeOut(200);
        }

        public partial class BasicCaret : Caret
        {
            public BasicCaret()
            {
                Colour = Color4.Transparent;

                InternalChild = new Container
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.Both,
                    Height = 0.9f,
                    CornerRadius = 1f,
                    Masking = true,
                    Child = new Box { RelativeSizeAxes = Axes.Both },
                };
            }

            public override void Hide() => this.FadeOut(200);

            public float CaretWidth { get; set; }

            public Color4 SelectionColour { get; set; }

            public override void DisplayAt(Vector2 position, float? selectionWidth)
            {
                if (selectionWidth != null)
                {
                    this.MoveTo(new Vector2(position.X, position.Y), 60, Easing.Out);
                    this.ResizeWidthTo(selectionWidth.Value + CaretWidth / 2, caret_move_time, Easing.Out);
                    this
                        .FadeTo(0.5f, 200, Easing.Out)
                        .FadeColour(SelectionColour, 200, Easing.Out);
                }
                else
                {
                    this.MoveTo(new Vector2(position.X - CaretWidth / 2, position.Y), 60, Easing.Out);
                    this.ResizeWidthTo(CaretWidth, caret_move_time, Easing.Out);
                    this
                        .FadeColour(Color4.White, 200, Easing.Out)
                        .Loop(c => c.FadeTo(0.7f).FadeTo(0.4f, 500, Easing.InOutSine));
                }
            }
        }
    }
}
