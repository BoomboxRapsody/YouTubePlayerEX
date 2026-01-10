using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osuTK;
using osuTK.Graphics;
using YouTubePlayerEX.App.Graphics.Sprites;

namespace YouTubePlayerEX.App.Graphics.UserInterfaceV2
{
    public partial class FormCheckBox : CompositeDrawable, IHasCurrentValue<bool>, IFormControl
    {
        public Bindable<bool> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private readonly BindableWithCurrent<bool> current = new BindableWithCurrent<bool>();

        /// <summary>
        /// Caption describing this slider bar, displayed on top of the controls.
        /// </summary>
        public LocalisableString Caption { get; init; }

        /// <summary>
        /// Hint text containing an extended description of this slider bar, displayed in a tooltip when hovering the caption.
        /// </summary>
        public LocalisableString HintText { get; init; }

        private Box background = null!;
        private FormFieldCaption caption = null!;
        private AdaptiveSpriteText text = null!;

        private Sample? sampleChecked;
        private Sample? sampleUnchecked;
        private Sample? sampleDisabled;

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
                                    RelativeSizeAxes = Axes.X,
                                },
                            },
                        },
                        new SwitchButton
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Current = Current,
                        },
                    },
                },
            };
            sampleChecked = audio.Samples.Get(@"UI/check-on");
            sampleUnchecked = audio.Samples.Get(@"UI/check-off");
            sampleDisabled = audio.Samples.Get(@"UI/default-select-disabled");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            current.BindValueChanged(_ =>
            {
                updateState();
                playSamples();
                background.FlashColour(ColourInfo.GradientVertical(Color4Extensions.FromHex(@"22252a"), Color4Extensions.FromHex(@"3d495c")), 800, Easing.OutQuint);

                ValueChanged?.Invoke();
            });
            current.BindDisabledChanged(_ => updateState(), true);
        }

        private void playSamples()
        {
            if (Current.Value)
                sampleChecked?.Play();
            else
                sampleUnchecked?.Play();
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

        protected override bool OnClick(ClickEvent e)
        {
            if (!Current.Disabled)
                Current.Value = !Current.Value;
            else
                sampleDisabled?.Play();

            return true;
        }

        private void updateState()
        {
            caption.Colour = Current.Disabled ? Color4Extensions.FromHex(@"5c6470") : Color4Extensions.FromHex(@"dbe3f0");
            text.Colour = Current.Disabled ? Color4Extensions.FromHex(@"5c6470") : Color4.White;

            text.Text = Current.Value ? "Enabled" : "Disabled";

            // use FadeColour to override any existing colour transform (i.e. FlashColour on click).
            background.FadeColour(IsHovered
                ? ColourInfo.GradientVertical(Color4Extensions.FromHex(@"22252a"), Color4Extensions.FromHex(@"29313d"))
                : Color4Extensions.FromHex(@"22252a"));

            BorderThickness = IsHovered ? 2 : 0;
            BorderColour = Current.Disabled ? Color4Extensions.FromHex(@"47566b") : Color4Extensions.FromHex(@"4d77b3");
        }

        public IEnumerable<LocalisableString> FilterTerms => Caption.Yield();

        public event Action? ValueChanged;

        public bool IsDefault => Current.IsDefault;

        public void SetDefault() => Current.SetDefault();

        public bool IsDisabled => Current.Disabled;
    }
}
