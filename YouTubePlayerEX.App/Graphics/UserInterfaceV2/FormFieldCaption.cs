using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osuTK;
using YouTubePlayerEX.App.Graphics.UserInterface;

namespace YouTubePlayerEX.App.Graphics.UserInterfaceV2
{
    public partial class FormFieldCaption : CompositeDrawable, IHasTooltip
    {
        private AdaptiveTextFlowContainer textFlow = null!;

        private LocalisableString caption;

        public LocalisableString Caption
        {
            get => caption;
            set
            {
                caption = value;

                if (IsLoaded)
                    updateDisplay();
            }
        }

        private LocalisableString tooltipText;

        public LocalisableString TooltipText
        {
            get => tooltipText;
            set
            {
                tooltipText = value;

                if (IsLoaded)
                    updateDisplay();
            }
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = textFlow = new AdaptiveTextFlowContainer(t => t.Font = YouTubePlayerEXApp.DefaultFont.With(size: 12, weight: "SemiBold"))
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateDisplay();
        }

        private void updateDisplay()
        {
            textFlow.Text = caption;

            if (TooltipText != default)
            {
                textFlow.AddArbitraryDrawable(new SpriteIcon
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Size = new Vector2(10),
                    Icon = FontAwesome.Solid.QuestionCircle,
                    Margin = new MarginPadding { Left = 5 },
                    Y = 1f,
                });
            }
        }
    }
}
