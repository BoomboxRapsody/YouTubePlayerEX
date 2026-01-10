using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osuTK;
using osuTK.Graphics;
using YouTubePlayerEX.App.Graphics.Sprites;
using YouTubePlayerEX.App.Graphics.UserInterface;

namespace YouTubePlayerEX.App.Graphics.UserInterfaceV2
{
    public partial class FormDropdown<T> : AdaptiveDropdown<T>, IFormControl
    {
        /// <summary>
        /// Caption describing this slider bar, displayed on top of the controls.
        /// </summary>
        public LocalisableString Caption { get; init; }

        /// <summary>
        /// Hint text containing an extended description of this slider bar, displayed in a tooltip when hovering the caption.
        /// </summary>
        public LocalisableString HintText { get; init; }

        /// <summary>
        /// The maximum height of the dropdown's menu.
        /// By default, this is set to 200px high. Set to <see cref="float.PositiveInfinity"/> to remove such limit.
        /// </summary>
        public float MaxHeight { get; set; } = 200;

        private FormDropdownHeader header = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;

            header.Caption = Caption;
            header.HintText = HintText;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Current.BindValueChanged(_ => ValueChanged?.Invoke());
        }

        public virtual IEnumerable<LocalisableString> FilterTerms
        {
            get
            {
                yield return Caption;

                foreach (var item in MenuItems)
                    yield return item.Text.Value;
            }
        }

        public event Action? ValueChanged;

        public bool IsDefault => Current.IsDefault;

        public void SetDefault() => Current.SetDefault();

        public bool IsDisabled => Current.Disabled;

        protected override DropdownHeader CreateHeader() => header = new FormDropdownHeader
        {
            Dropdown = this,
        };

        protected override DropdownMenu CreateMenu() => new FormDropdownMenu
        {
            MaxHeight = MaxHeight,
        };

        private partial class FormDropdownHeader : DropdownHeader
        {
            public FormDropdown<T> Dropdown { get; set; } = null!;

            protected override DropdownSearchBar CreateSearchBar() => SearchBar = new FormDropdownSearchBar();

            private LocalisableString captionText;
            private LocalisableString hintText;
            private LocalisableString labelText;

            public LocalisableString Caption
            {
                get => captionText;
                set
                {
                    captionText = value;

                    if (caption.IsNotNull())
                        caption.Caption = value;
                }
            }

            public LocalisableString HintText
            {
                get => hintText;
                set
                {
                    hintText = value;

                    if (caption.IsNotNull())
                        caption.TooltipText = value;
                }
            }

            protected override LocalisableString Label
            {
                get => labelText;
                set
                {
                    labelText = value;

                    if (label.IsNotNull())
                        label.Text = labelText;
                }
            }

            protected new FormDropdownSearchBar SearchBar { get; set; } = null!;

            private FormFieldCaption caption = null!;
            private AdaptiveSpriteText label = null!;
            private SpriteIcon chevron = null!;

            [BackgroundDependencyLoader]
            private void load()
            {
                Masking = true;
                CornerRadius = 5;

                Foreground.Padding = new MarginPadding(9);
                Foreground.Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 4),
                        Children = new Drawable[]
                        {
                            caption = new FormFieldCaption
                            {
                                Caption = Caption,
                                TooltipText = HintText,
                            },
                            label = new AdaptiveSpriteText
                            {
                                RelativeSizeAxes = Axes.X,
                            },
                        }
                    },
                    chevron = new SpriteIcon
                    {
                        Icon = FontAwesome.Solid.ChevronDown,
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        Size = new Vector2(16),
                        Margin = new MarginPadding { Right = 5 },
                    },
                };

                AddInternal(new HoverClickSounds
                {
                    Enabled = { BindTarget = Enabled },
                });
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Dropdown.Current.BindDisabledChanged(_ => updateState());
                SearchBar.SearchTerm.BindValueChanged(_ => updateState(), true);
                Dropdown.Menu.StateChanged += _ =>
                {
                    updateState();
                    updateChevron();
                };
                SearchBar.TextBox.OnCommit += (_, _) =>
                {
                    Background.FlashColour(ColourInfo.GradientVertical(Color4Extensions.FromHex(@"22252a"), Color4Extensions.FromHex(@"3d485c")), 800, Easing.OutQuint);
                };
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
                label.Alpha = string.IsNullOrEmpty(SearchBar.SearchTerm.Value) ? 1 : 0;

                caption.Colour = Dropdown.Current.Disabled ? Color4Extensions.FromHex(@"5c6470") : Color4Extensions.FromHex(@"dbe3f0");
                label.Colour = Dropdown.Current.Disabled ? Color4Extensions.FromHex(@"5c6470") : Color4.White;
                chevron.Colour = Dropdown.Current.Disabled ? Color4Extensions.FromHex(@"5c6470") : Color4.White;
                DisabledColour = Colour4.White;

                bool dropdownOpen = Dropdown.Menu.State == MenuState.Open;

                BorderThickness = IsHovered || dropdownOpen ? 2 : 0;

                if (Dropdown.Current.Disabled)
                    BorderColour = Color4Extensions.FromHex(@"47556b");
                else
                    BorderColour = dropdownOpen ? Color4Extensions.FromHex(@"66a1ff") : Color4Extensions.FromHex(@"4d74b3");

                if (dropdownOpen)
                    Background.Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex(@"22252a"), Color4Extensions.FromHex(@"333c4d"));
                else if (IsHovered)
                    Background.Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex(@"22252a"), Color4Extensions.FromHex(@"29303d"));
                else
                    Background.Colour = Color4Extensions.FromHex(@"22252a");
            }

            private void updateChevron()
            {
                bool open = Dropdown.Menu.State == MenuState.Open;
                chevron.ScaleTo(open ? new Vector2(1f, -1f) : Vector2.One, 300, Easing.OutQuint);
            }
        }

        private partial class FormDropdownSearchBar : DropdownSearchBar
        {
            public FormTextBox.InnerTextBox TextBox { get; private set; } = null!;

            protected override void PopIn() => this.FadeIn();
            protected override void PopOut() => this.FadeOut();

            protected override TextBox CreateTextBox() => TextBox = new FormTextBox.InnerTextBox();

            [BackgroundDependencyLoader]
            private void load()
            {
                TextBox.Anchor = Anchor.BottomLeft;
                TextBox.Origin = Anchor.BottomLeft;
                TextBox.RelativeSizeAxes = Axes.X;
                TextBox.Margin = new MarginPadding(9);
            }
        }

        private partial class FormDropdownMenu : AdaptiveDropdownMenu
        {
            [BackgroundDependencyLoader]
            private void load()
            {
                ItemsContainer.Padding = new MarginPadding(9);
                Margin = new MarginPadding { Top = 5 };

                MaskingContainer.BorderThickness = 2;
                MaskingContainer.BorderColour = Color4Extensions.FromHex(@"66a1ff");
            }
        }
    }

    public partial class FormEnumDropdown<T> : FormDropdown<T>
        where T : struct, Enum
    {
        public FormEnumDropdown()
        {
            Items = Enum.GetValues<T>();
        }
    }
}
