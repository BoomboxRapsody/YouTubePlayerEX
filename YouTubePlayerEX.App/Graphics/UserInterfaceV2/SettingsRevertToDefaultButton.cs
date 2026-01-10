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

namespace YouTubePlayerEX.App.Graphics.UserInterfaceV2
{
    public partial class SettingsRevertToDefaultButton : AdaptiveClickableContainer
    {
        public const float WIDTH = 32;

        public float IconSize { get; init; } = 14;

        private Box background = null!;
        private Sprite spriteIcon = null!;

        // this is done to ensure a click on this button doesn't trigger focus on a parent element which contains the button.
        public override bool AcceptsFocus => true;

        public SettingsRevertToDefaultButton()
        {
            Size = new Vector2(WIDTH, 50);
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textureStore)
        {
            Masking = true;
            CornerRadius = 5;
            CornerExponent = 2.5f;

            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4Extensions.FromHex(@"393e47"),
                },
                spriteIcon = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = Color4Extensions.FromHex(@"b8c6e0"),
                    Texture = textureStore.Get("undo"),
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

        public override LocalisableString TooltipText => "Revert to default";

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
            spriteIcon.FadeColour(IsHovered ? Color4Extensions.FromHex(@"dbe3f0") : Color4Extensions.FromHex(@"b8c6e0"), 300, Easing.OutQuint);
            background.FadeColour(IsHovered ? Color4Extensions.FromHex(@"454b54") : Color4Extensions.FromHex(@"393e47"), 300, Easing.OutQuint);
        }
    }
}
