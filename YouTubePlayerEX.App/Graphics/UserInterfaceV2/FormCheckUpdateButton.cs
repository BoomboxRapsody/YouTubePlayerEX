using System;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osuTK;
using osuTK.Graphics;
using YouTubePlayerEX.App.Graphics.Sprites;
using YouTubePlayerEX.App.Graphics.UserInterface;

namespace YouTubePlayerEX.App.Graphics.UserInterfaceV2
{
    public partial class FormCheckUpdateButton : CompositeDrawable, IFormControl
    {
        /// <summary>
        /// Caption describing this button, displayed on the left of it.
        /// </summary>
        public LocalisableString Caption { get; init; }

        private LocalisableString textValue;

        /// <summary>
        /// Caption describing this button, displayed on the left of it.
        /// </summary>
        public LocalisableString Text
        {
            get => textValue;
            set
            {
                if (textValue.Equals(value))
                    return;

                textValue = value;

                if (text != null)
                    text.Text = value;
            }
        }

        /// <summary>
        /// Sets text inside the button.
        /// </summary>
        public LocalisableString ButtonText { get; init; }

        /// <summary>
        /// Hint text containing an extended description of this slider bar, displayed in a tooltip when hovering the caption.
        /// </summary>
        public LocalisableString HintText { get; init; }

        /// <summary>
        /// Sets a custom button icon. Not shown when <see cref="ButtonText"/> is set.
        /// </summary>
        public IconUsage ButtonIcon { get; init; } = FontAwesome.Solid.Sync;

        private Box background = null!;
        private FormFieldCaption caption = null!;
        private AdaptiveSpriteText text = null!;

        /// <summary>
        /// The action to invoke when the button is clicked.
        /// </summary>
        public Action? Action { get; set; }

        /// <summary>
        /// Whether the button is enabled.
        /// </summary>
        public readonly BindableBool Enabled = new BindableBool(true);

        private Button button = null!;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Masking = true;
            CornerRadius = 5;
            CornerExponent = 2.5f;

            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4Extensions.FromHex(@"22252a"),
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(9),
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Padding = new MarginPadding { Right = SwitchButton.WIDTH + 5 },
                            Spacing = new Vector2(0f, 4f),
                            Children = new Drawable[]
                            {
                                caption = new FormFieldCaption
                                {
                                    Caption = Caption,
                                    TooltipText = HintText,
                                },
                                text = new AdaptiveSpriteText
                                {
                                    Text = Text,
                                    RelativeSizeAxes = Axes.X,
                                },
                            },
                        },
                        button = new Button
                        {
                            Action = () => Action?.Invoke(),
                            Text = ButtonText,
                            Icon = ButtonIcon,
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Enabled = { BindTarget = Enabled },
                            Height = 30,
                        },
                    },
                },
            };

            if (ButtonText == default)
            {
                text.Padding = new MarginPadding { Right = 100 };
                button.Width = 90;
            }
            else
            {
                text.Width = 0.55f;
                text.Padding = new MarginPadding { Right = 10 };
                button.RelativeSizeAxes = Axes.X;
                button.Width = 0.45f;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateState();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);
            updateState();
        }

        private void updateState()
        {
            caption.Colour = Color4Extensions.FromHex(@"dbe3f0");
            text.Colour = Color4.White;

            // use FadeColour to override any existing colour transform (i.e. FlashColour on click).
            background.FadeColour(IsHovered
                ? ColourInfo.GradientVertical(Color4Extensions.FromHex(@"22252a"), Color4Extensions.FromHex(@"29313d"))
                : Color4Extensions.FromHex(@"22252a"));

            BorderThickness = IsHovered ? 2 : 0;
            BorderColour = Color4Extensions.FromHex(@"4d77b3");
        }

        public IEnumerable<LocalisableString> FilterTerms => Caption.Yield();

        public event Action? ValueChanged;

        public bool IsDefault => true;

        public void SetDefault()
        {

        }

        public bool IsDisabled => false;

        public partial class Button : AdaptiveButtonV2
        {
            private TrianglesV2? triangles { get; set; }

            protected override float HoverLayerFinalAlpha => 0;

            private Color4? triangleGradientSecondColour;

            public override Color4 BackgroundColour
            {
                get => base.BackgroundColour;
                set
                {
                    base.BackgroundColour = value;
                    triangleGradientSecondColour = BackgroundColour.Lighten(0.2f);
                    updateColours();
                }
            }

            public IconUsage Icon { get; init; }

            [BackgroundDependencyLoader]
            private void load()
            {
                DefaultBackgroundColour = Color4Extensions.FromHex(@"336ecc");
                triangleGradientSecondColour ??= DefaultBackgroundColour.Lighten(0.2f);

                if (Text == default)
                {
                    Add(new SpriteIcon
                    {
                        Icon = Icon,
                        Size = new Vector2(16),
                        Shadow = true,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    });
                }
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Content.CornerRadius = 4;

                Add(triangles = new TrianglesV2
                {
                    Thickness = 0.02f,
                    SpawnRatio = 0.6f,
                    RelativeSizeAxes = Axes.Both,
                    Depth = float.MaxValue,
                });

                updateColours();
            }

            private void updateColours()
            {
                if (triangles == null)
                    return;

                Debug.Assert(triangleGradientSecondColour != null);

                triangles.Colour = ColourInfo.GradientVertical(triangleGradientSecondColour.Value, BackgroundColour);
            }

            protected override bool OnHover(HoverEvent e)
            {
                Debug.Assert(triangleGradientSecondColour != null);

                Background.FadeColour(triangleGradientSecondColour.Value, 300, Easing.OutQuint);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                Background.FadeColour(BackgroundColour, 300, Easing.OutQuint);
                base.OnHoverLost(e);
            }
        }
    }
}
