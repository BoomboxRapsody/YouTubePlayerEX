using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osuTK.Graphics;
using YouTubePlayerEX.App.Extensions;
using YouTubePlayerEX.App.Graphics.Sprites;
using YouTubePlayerEX.App.Graphics.UserInterface;
using YouTubePlayerEX.App.Localisation;
using Vector2 = osuTK.Vector2;

namespace YouTubePlayerEX.App.Graphics.UserInterfaceV2
{
    public partial class FormSliderBar<T> : CompositeDrawable, IHasCurrentValue<T>, IFormControl
        where T : struct, INumber<T>, IMinMaxValue<T>
    {
        public Bindable<T> Current
        {
            get => current.Current;
            set
            {
                current.Current = value;
                currentNumberInstantaneous.Default = current.Default;
            }
        }

        private readonly BindableNumberWithCurrent<T> current = new BindableNumberWithCurrent<T>();

        private readonly BindableNumber<T> currentNumberInstantaneous = new BindableNumber<T>();

        /// <summary>
        /// Whether changes to the value should instantaneously transfer to outside bindables.
        /// If <see langword="false"/>, the transfer will happen on text box commit (explicit, or implicit via focus loss), or on slider commit.
        /// </summary>
        public bool TransferValueOnCommit { get; set; }

        private CompositeDrawable? tabbableContentContainer;

        public CompositeDrawable? TabbableContentContainer
        {
            set
            {
                tabbableContentContainer = value;

                if (textBox.IsNotNull())
                    textBox.TabbableContentContainer = tabbableContentContainer;
            }
        }

        private LocalisableString caption;

        /// <summary>
        /// Caption describing this slider bar, displayed on top of the controls.
        /// </summary>
        public LocalisableString Caption
        {
            get => caption;
            set
            {
                caption = value;

                if (IsLoaded)
                    captionText.Caption = value;
            }
        }

        /// <summary>
        /// Hint text containing an extended description of this slider bar, displayed in a tooltip when hovering the caption.
        /// </summary>
        public LocalisableString HintText { get; init; }

        private float keyboardStep;

        /// <summary>
        /// A custom step value for each key press which actuates a change on this control.
        /// </summary>
        public float KeyboardStep
        {
            get => keyboardStep;
            set
            {
                keyboardStep = value;
                if (IsLoaded)
                    slider.KeyboardStep = value;
            }
        }

        /// <summary>
        /// Whether to format the tooltip as a percentage or the actual value.
        /// </summary>
        public bool DisplayAsPercentage { get; init; }

        /// <summary>
        /// Whether sound effects should play when adjusting this slider.
        /// </summary>
        public bool PlaySamplesOnAdjust { get; init; }

        /// <summary>
        /// The string formatting function to use for the value label.
        /// </summary>
        public Func<T, LocalisableString> LabelFormat { get; init; }

        /// <summary>
        /// The string formatting function to use for the slider's tooltip text.
        /// If not provided, <see cref="LabelFormat"/> is used.
        /// </summary>
        public Func<T, LocalisableString> TooltipFormat { get; init; }

        private Box background = null!;
        private Box flashLayer = null!;
        private FormTextBox.InnerTextBox textBox = null!;
        private AdaptiveSpriteText valueLabel = null!;
        private InnerSlider slider = null!;
        private FormFieldCaption captionText = null!;
        private IFocusManager focusManager = null!;

        private readonly Bindable<Language> currentLanguage = new Bindable<Language>();

        public FormSliderBar()
        {
            LabelFormat ??= defaultLabelFormat;
            TooltipFormat ??= v => LabelFormat(v);
        }

        [BackgroundDependencyLoader]
        private void load(YouTubePlayerEXApp? game)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Masking = true;
            CornerRadius = 5;

            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4Extensions.FromHex(@"22252a"),
                },
                flashLayer = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.Transparent,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding
                    {
                        Vertical = 5,
                        Left = 9,
                        Right = 5,
                    },
                    Children = new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0f, 4f),
                            Width = 0.5f,
                            Padding = new MarginPadding
                            {
                                Right = 10,
                                Vertical = 4,
                            },
                            Children = new Drawable[]
                            {
                                captionText = new FormFieldCaption
                                {
                                    TooltipText = HintText,
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Children = new Drawable[]
                                    {
                                        textBox = new FormNumberBox.InnerNumberBox(allowDecimals: true)
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            // the textbox is hidden when the control is unfocused,
                                            // but clicking on the label should reach the textbox,
                                            // therefore make it always present.
                                            AlwaysPresent = true,
                                            CommitOnFocusLost = true,
                                            SelectAllOnFocus = true,
                                            OnInputError = () =>
                                            {
                                                flashLayer.Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex(@"cc3333").Opacity(0), Color4Extensions.FromHex(@"cc3333"));
                                                flashLayer.FadeOutFromOne(200, Easing.OutQuint);
                                            },
                                            TabbableContentContainer = tabbableContentContainer,
                                        },
                                        valueLabel = new TruncatingSpriteText
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Padding = new MarginPadding { Right = 5 },
                                        },
                                    },
                                },
                            },
                        },
                        slider = new InnerSlider
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            RelativeSizeAxes = Axes.X,
                            Width = 0.5f,
                            Current = currentNumberInstantaneous,
                            OnCommit = () => current.Value = currentNumberInstantaneous.Value,
                            TooltipFormat = TooltipFormat,
                            DisplayAsPercentage = DisplayAsPercentage,
                            PlaySamplesOnAdjust = PlaySamplesOnAdjust,
                        }
                    },
                },
            };

            if (game != null)
                currentLanguage.BindTo(game.CurrentLanguage);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            slider.KeyboardStep = keyboardStep;
            captionText.Caption = caption;

            focusManager = GetContainingFocusManager()!;

            textBox.Focused.BindValueChanged(_ => updateState());
            textBox.OnCommit += textCommitted;
            textBox.Current.BindValueChanged(textChanged);

            slider.IsDragging.BindValueChanged(_ => updateState());
            slider.Focused.BindValueChanged(_ => updateState());

            current.ValueChanged += e =>
            {
                currentNumberInstantaneous.Value = e.NewValue;
                ValueChanged?.Invoke();
            };

            current.MinValueChanged += v => currentNumberInstantaneous.MinValue = v;
            current.MaxValueChanged += v => currentNumberInstantaneous.MaxValue = v;
            current.PrecisionChanged += v => currentNumberInstantaneous.Precision = v;
            current.DisabledChanged += disabled =>
            {
                if (disabled)
                {
                    // revert any changes before disabling to make sure we are in a consistent state.
                    currentNumberInstantaneous.Value = current.Value;
                }

                currentNumberInstantaneous.Disabled = disabled;
                updateState();
            };

            current.CopyTo(currentNumberInstantaneous);
            currentLanguage.BindValueChanged(_ => Schedule(updateValueDisplay));
            currentNumberInstantaneous.BindDisabledChanged(_ => updateState());
            currentNumberInstantaneous.BindValueChanged(e =>
            {
                if (!TransferValueOnCommit)
                    current.Value = e.NewValue;

                updateState();
                updateValueDisplay();
            }, true);
        }

        private bool updatingFromTextBox;

        private void textChanged(ValueChangedEvent<string> change)
        {
            tryUpdateSliderFromTextBox();
        }

        private void textCommitted(TextBox t, bool isNew)
        {
            tryUpdateSliderFromTextBox();
            // If the attempted update above failed, restore text box to match the slider.
            currentNumberInstantaneous.TriggerChange();
            current.Value = currentNumberInstantaneous.Value;

            flashLayer.Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex(@"3d485c").Opacity(0), Color4Extensions.FromHex(@"3d485c"));
            flashLayer.FadeOutFromOne(800, Easing.OutQuint);
        }

        private void tryUpdateSliderFromTextBox()
        {
            updatingFromTextBox = true;

            try
            {
                switch (currentNumberInstantaneous)
                {
                    case Bindable<int> bindableInt:
                        bindableInt.Value = int.Parse(textBox.Current.Value);
                        break;

                    case Bindable<double> bindableDouble:
                        bindableDouble.Value = double.Parse(textBox.Current.Value);
                        break;

                    default:
                        currentNumberInstantaneous.Parse(textBox.Current.Value, CultureInfo.CurrentCulture);
                        break;
                }
            }
            catch
            {
                // ignore parsing failures.
                // sane state will eventually be restored by a commit (either explicit, or implicit via focus loss).
            }

            updatingFromTextBox = false;
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
                focusManager.ChangeFocus(textBox);
            return true;
        }

        private void updateState()
        {
            bool childHasFocus = slider.Focused.Value || textBox.Focused.Value;

            textBox.ReadOnly = currentNumberInstantaneous.Disabled;
            textBox.Alpha = textBox.Focused.Value ? 1 : 0;
            valueLabel.Alpha = textBox.Focused.Value ? 0 : 1;

            captionText.Colour = currentNumberInstantaneous.Disabled ? Color4Extensions.FromHex(@"5c6370") : Color4Extensions.FromHex(@"dbe3f0");
            textBox.Colour = currentNumberInstantaneous.Disabled ? Color4Extensions.FromHex(@"5c6370") : Color4.White;
            valueLabel.Colour = currentNumberInstantaneous.Disabled ? Color4Extensions.FromHex(@"5c6370") : Color4.White;

            BorderThickness = childHasFocus || IsHovered || slider.IsDragging.Value ? 2 : 0;

            if (Current.Disabled)
                BorderColour = Color4Extensions.FromHex(@"47556b");
            else
                BorderColour = childHasFocus ? Color4Extensions.FromHex(@"66a1ff") : Color4Extensions.FromHex(@"4d74b3");

            if (childHasFocus)
                background.Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex(@"22252a"), Color4Extensions.FromHex(@"333c4d"));
            else if (IsHovered || slider.IsDragging.Value)
                background.Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex(@"22252a"), Color4Extensions.FromHex(@"29303d"));
            else
                background.Colour = Color4Extensions.FromHex(@"22252a");
        }

        private void updateValueDisplay()
        {
            if (updatingFromTextBox) return;

            textBox.Text = currentNumberInstantaneous.Value.ToStandardFormattedString(AdaptiveSliderBar<T>.MAX_DECIMAL_DIGITS);
            valueLabel.Text = LabelFormat(currentNumberInstantaneous.Value);
        }

        private LocalisableString defaultLabelFormat(T value) => currentNumberInstantaneous.Value.ToStandardFormattedString(AdaptiveSliderBar<T>.MAX_DECIMAL_DIGITS, DisplayAsPercentage);

        private partial class InnerSlider : AdaptiveSliderBar<T>
        {
            public BindableBool Focused { get; } = new BindableBool();

            public BindableBool IsDragging { get; set; } = new BindableBool();

            public Action? OnCommit { get; set; }

            public sealed override LocalisableString TooltipText => base.TooltipText;

            public required Func<T, LocalisableString> TooltipFormat { get; init; }

            private Box leftBox = null!;
            private Box rightBox = null!;
            private InnerSliderNub nub = null!;
            private HoverClickSounds sounds = null!;
            public const float NUB_WIDTH = 10;

            public bool PlaySamplesOnAdjust { get; set; } = true;

            [BackgroundDependencyLoader]
            private void load()
            {
                Height = 40;
                RelativeSizeAxes = Axes.X;
                RangePadding = NUB_WIDTH / 2;

                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 5,
                        Children = new Drawable[]
                        {
                            leftBox = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                            },
                            rightBox = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                            },
                        },
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Horizontal = RangePadding, },
                        Child = nub = new InnerSliderNub
                        {
                            ResetToDefault = () =>
                            {
                                if (!Current.Disabled)
                                    Current.SetDefault();
                            }
                        }
                    },
                    sounds = new HoverClickSounds()
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                Current.BindDisabledChanged(_ => updateState(), true);
            }

            protected override void UpdateAfterChildren()
            {
                base.UpdateAfterChildren();
                leftBox.Width = Math.Clamp(RangePadding + nub.DrawPosition.X, 0, Math.Max(0, DrawWidth)) / DrawWidth;
                rightBox.Width = Math.Clamp(DrawWidth - nub.DrawPosition.X - RangePadding, 0, Math.Max(0, DrawWidth)) / DrawWidth;
            }

            protected override bool OnDragStart(DragStartEvent e)
            {
                bool dragging = base.OnDragStart(e);
                IsDragging.Value = dragging;
                updateState();
                return dragging;
            }

            protected override void OnDragEnd(DragEndEvent e)
            {
                base.OnDragEnd(e);
                IsDragging.Value = false;
                updateState();
            }

            protected override bool OnHover(HoverEvent e)
            {
                updateState();
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                updateState();
                base.OnHoverLost(e);
            }

            protected override void OnFocus(FocusEvent e)
            {
                updateState();
                Focused.Value = true;
                base.OnFocus(e);
            }

            protected override void OnFocusLost(FocusLostEvent e)
            {
                updateState();
                Focused.Value = false;
                base.OnFocusLost(e);
            }

            private void updateState()
            {
                sounds.Enabled.Value = !Current.Disabled;
                rightBox.Colour = Color4Extensions.FromHex(@"17191c");

                if (Current.Disabled)
                {
                    leftBox.Colour = Color4Extensions.FromHex(@"333d4d");
                    nub.Colour = Color4Extensions.FromHex(@"47556b");
                }
                else
                {
                    leftBox.Colour = HasFocus || IsHovered || IsDragged ? Color4Extensions.FromHex(@"66a1ff").Opacity(0.5f) : Color4Extensions.FromHex(@"66a1ff").Opacity(0.3f);
                    nub.Colour = HasFocus || IsHovered || IsDragged ? Color4Extensions.FromHex(@"66a1ff") : Color4Extensions.FromHex(@"4d74b3");
                }
            }

            protected override void UpdateValue(float value)
            {
                nub.MoveToX(value, 200, Easing.OutPow10);
            }

            protected override bool Commit()
            {
                bool result = base.Commit();

                if (result)
                    OnCommit?.Invoke();

                return result;
            }

            protected sealed override LocalisableString GetTooltipText(T value) => TooltipFormat(value);
        }

        private partial class InnerSliderNub : Circle
        {
            public Action? ResetToDefault { get; set; }

            [BackgroundDependencyLoader]
            private void load()
            {
                Width = InnerSlider.NUB_WIDTH;
                RelativeSizeAxes = Axes.Y;
                RelativePositionAxes = Axes.X;
                Origin = Anchor.TopCentre;
            }

            protected override bool OnClick(ClickEvent e) => true; // must be handled for double click handler to ever fire

            protected override bool OnDoubleClick(DoubleClickEvent e)
            {
                ResetToDefault?.Invoke();
                return true;
            }
        }

        public IEnumerable<LocalisableString> FilterTerms => new[] { Caption, HintText };

        public event Action? ValueChanged;

        public bool IsDefault => Current.IsDefault;

        public void SetDefault() => Current.SetDefault();

        public bool IsDisabled => Current.Disabled;
    }
}
