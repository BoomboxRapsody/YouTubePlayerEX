using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osuTK.Graphics;
using System;
using osu.Framework.Localisation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Effects;

namespace YouTubePlayerEX.App.Graphics.UserInterface
{
    public partial class AdaptiveButton : AdaptiveClickableContainer
    {
        public Action<AdaptiveButton>? ClickAction { get; set; }

        public LocalisableString Text
        {
            get => SpriteText.Text;
            set => SpriteText.Text = value;
        }

        public Color4 BackgroundColour
        {
            get => Background.Colour;
            set => Background.FadeColour(value);
        }

        private Color4? flashColour;

        /// <summary>
        /// The colour the background will flash with when this button is clicked.
        /// </summary>
        public Color4 FlashColour
        {
            get => flashColour ?? BackgroundColour;
            set => flashColour = value;
        }

        /// <summary>
        /// The additive colour that is applied to the background when hovered.
        /// </summary>
        public Color4 HoverColour
        {
            get => Hover.Colour;
            set => Hover.FadeColour(value);
        }

        private Color4 disabledColour = Color4.Gray;

        /// <summary>
        /// The additive colour that is applied to this button when disabled.
        /// </summary>
        public Color4 DisabledColour
        {
            get => disabledColour;
            set
            {
                if (disabledColour == value)
                    return;

                disabledColour = value;
                Enabled.TriggerChange();
            }
        }

        /// <summary>
        /// The duration of the transition when hovering.
        /// </summary>
        public double HoverFadeDuration { get; set; } = 200;

        /// <summary>
        /// The duration of the flash when this button is clicked.
        /// </summary>
        public double FlashDuration { get; set; } = 200;

        /// <summary>
        /// The duration of the transition when toggling the Enabled state.
        /// </summary>
        public double DisabledFadeDuration { get; set; } = 200;

        protected Box Hover;
        protected Box Background;
        protected SpriteText SpriteText;
        private readonly Container content;

        public AdaptiveButton(HoverSampleSet hoverSampleSet = HoverSampleSet.Default)
            : base(hoverSampleSet)
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
                    Background = new Box
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Transparent,
                        Alpha = 0,
                    },
                    Hover = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.SkyBlue,
                        Blending = BlendingParameters.Additive,
                        Alpha = 0,
                    },
                    SpriteText = CreateText()
                }
            });

            Enabled.BindValueChanged(enabledChanged, true);
        }

        protected virtual SpriteText CreateText() => new SpriteText
        {
            Depth = -1,
            Origin = Anchor.Centre,
            Anchor = Anchor.Centre,
            Font = YouTubePlayerEXApp.DefaultFont,
            Colour = Color4.White,
        };

        protected override bool OnHover(HoverEvent e)
        {
            Hover.FadeIn(500, Easing.OutQuint);

            return base.OnHover(e);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            Content.ScaleTo(0.9f, 2000, Easing.OutQuint);
            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            Content.ScaleTo(1, 1000, Easing.OutElastic);
            base.OnMouseUp(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            base.OnHoverLost(e);

            Hover.FadeOut(500, Easing.OutQuint);
        }

        private void enabledChanged(ValueChangedEvent<bool> e)
        {
            this.FadeColour(e.NewValue ? Color4.White : DisabledColour, DisabledFadeDuration, Easing.OutQuint);
        }

        private void trigger()
        {
            ClickAction?.Invoke(this);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (!IsHovered)
                Hover.FadeOutFromOne(1600);

            Hover.FlashColour(FlashColour, 800, Easing.OutQuint);
            trigger();

            return base.OnClick(e);
        }
    }
}
