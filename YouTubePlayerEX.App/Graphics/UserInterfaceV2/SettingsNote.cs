using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osuTK.Graphics;
using YouTubePlayerEX.App.Graphics.UserInterface;

namespace YouTubePlayerEX.App.Graphics.UserInterfaceV2
{
    public sealed partial class SettingsNote : CompositeDrawable
    {
        public readonly Bindable<Data?> Current = new Bindable<Data?>();

        private Box background = null!;
        private AdaptiveTextFlowContainer text = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeDuration = 300;
            AutoSizeEasing = Easing.OutQuint;

            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding { Top = 5, Bottom = 5 },
                Child = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    CornerRadius = 5,
                    CornerExponent = 2.5f,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        background = new Box
                        {
                            Colour = Color4.Black,
                            RelativeSizeAxes = Axes.Both,
                        },
                        text = new AdaptiveTextFlowContainer(s => s.Font = YouTubePlayerEXApp.DefaultFont.With(weight: "SemiBold"))
                        {
                            Padding = new MarginPadding(8),
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                        },
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Current.BindValueChanged(_ => updateDisplay(), true);
            FinishTransforms(true);
        }

        private void updateDisplay()
        {
            // Explicitly use ClearTransforms to clear any existing auto-size transform before modifying size / flag.
            ClearTransforms();

            if (Current.Value == null)
            {
                AutoSizeAxes = Axes.None;
                this.ResizeHeightTo(0, 300, Easing.OutQuint);
                this.FadeOut(250, Easing.OutQuint);
                return;
            }

            AutoSizeAxes = Axes.Y;
            this.FadeIn(250, Easing.OutQuint);

            switch (Current.Value.Type)
            {
                case Type.Informational:
                    background.Colour = Color4Extensions.FromHex(@"3d485c");
                    text.Colour = Color4Extensions.FromHex(@"dbe2f0");
                    break;

                case Type.Warning:
                    background.Colour = Color4Extensions.FromHex(@"ffd966");
                    text.Colour = Color4Extensions.FromHex(@"22252a");
                    break;

                case Type.Critical:
                    background.Colour = Color4Extensions.FromHex(@"ff6666");
                    text.Colour = Color4Extensions.FromHex(@"22252a");
                    break;
            }

            text.Text = Current.Value.Text;
        }

        public record Data(LocalisableString Text, Type Type);

        public enum Type
        {
            Informational,
            Warning,
            Critical,
        }
    }
}
