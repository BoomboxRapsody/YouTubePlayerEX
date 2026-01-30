using System.Reflection;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osuTK;
using YouTubePlayerEX.App.Graphics.UserInterface;
using YouTubePlayerEX.App.Localisation;

namespace YouTubePlayerEX.App.Graphics.UserInterfaceV2
{
    public partial class SettingsRevertToDefaultButton : AdaptiveClickableContainer
    {
        public const float WIDTH = 28;

        public float IconSize { get; init; } = 10;

        private Box background = null!;
        private SpriteIcon spriteIcon = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        // this is done to ensure a click on this button doesn't trigger focus on a parent element which contains the button.
        public override bool AcceptsFocus => true;

        public SettingsRevertToDefaultButton()
        {
            RelativeSizeAxes = Axes.Y;
            Width = WIDTH;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Masking = true;
            CornerRadius = 5;
            CornerExponent = 2.5f;

            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background3,
                },
                spriteIcon = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = colourProvider.Light1,
                    Icon = FontAwesome.Solid.Undo,
                    Margin = new MarginPadding { Left = 12, Right = 5 },
                    Size = new Vector2(IconSize),
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Enabled.BindValueChanged(_ => updateDisplay(), true);
        }

        public override LocalisableString TooltipText => YTPlayerEXStrings.RevertToDefault;

        protected override bool OnHover(HoverEvent e)
        {
            updateDisplay();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateDisplay();
            base.OnHoverLost(e);
        }

        public override void Show()
        {
            this.FadeIn().MoveToX(WIDTH - 10, 200, Easing.OutElasticQuarter);
        }

        public override void Hide()
        {
            this.MoveToX(0, 120, Easing.OutExpo).Then().FadeOut();
        }

        private void updateDisplay()
        {
            spriteIcon.FadeColour(IsHovered ? colourProvider.Content2 : colourProvider.Light1, 300, Easing.OutQuint);
            background.FadeColour(IsHovered ? colourProvider.Background2 : colourProvider.Background3, 300, Easing.OutQuint);
        }
    }
}
