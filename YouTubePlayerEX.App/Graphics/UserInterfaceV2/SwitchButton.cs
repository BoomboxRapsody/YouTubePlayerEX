using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osuTK;
using osuTK.Graphics;

namespace YouTubePlayerEX.App.Graphics.UserInterfaceV2
{
    public partial class SwitchButton : Checkbox
    {
        public const float WIDTH = 45;

        private const float border_thickness = 4.5f;
        private const float padding = 1.25f;

        private readonly Box fill;
        private readonly Container nubContainer;
        private readonly Drawable nub;
        private readonly CircularContainer content;

        private Sample? sampleChecked;
        private Sample? sampleUnchecked;

        public SwitchButton()
        {
            Size = new Vector2(WIDTH, 20);

            InternalChild = content = new CircularContainer
            {
                RelativeSizeAxes = Axes.Both,
                BorderColour = Color4.White,
                BorderThickness = border_thickness,
                Masking = true,
                Children = new Drawable[]
                {
                    fill = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        AlwaysPresent = true,
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding(border_thickness + padding),
                        Child = nubContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = nub = new Circle
                            {
                                RelativeSizeAxes = Axes.Both,
                                FillMode = FillMode.Fit,
                                Masking = true,
                            }
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader(true)]
        private void load(AudioManager audio)
        {
            sampleChecked = audio.Samples.Get(@"UI/check-on");
            sampleUnchecked = audio.Samples.Get(@"UI/check-off");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindDisabledChanged(_ => updateColours());
            Current.BindValueChanged(_ => updateState(), true);

            FinishTransforms(true);
        }

        private void updateState()
        {
            nub.MoveToX(Current.Value ? nubContainer.DrawWidth - nub.DrawWidth : 0, 200, Easing.OutQuint);

            updateColours();
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateColours();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateColours();
            base.OnHoverLost(e);
        }

        protected override void OnUserChange(bool value)
        {
            base.OnUserChange(value);

            if (value)
                sampleChecked?.Play();
            else
                sampleUnchecked?.Play();
        }

        private void updateColours()
        {
            ColourInfo borderColour;
            ColourInfo switchColour;

            if (Current.Disabled)
            {
                borderColour = Color4Extensions.FromHex(@"3d4a5c");
                switchColour = Color4Extensions.FromHex(@"47566b");
                fill.Colour = Color4Extensions.FromHex(@"1f252e");
            }
            else
            {
                bool hover = IsHovered && !Current.Disabled;

                borderColour = hover ? Color4Extensions.FromHex(@"66a3ff").Opacity(0.5f) : Color4Extensions.FromHex(@"66a3ff").Opacity(0.3f);
                switchColour = hover || Current.Value ? Color4Extensions.FromHex(@"66a3ff") : Color4Extensions.FromHex(@"4d76b3");

                if (!Current.Value)
                {
                    borderColour = borderColour.MultiplyAlpha(0.8f);
                    switchColour = switchColour.MultiplyAlpha(0.8f);
                }

                fill.Colour = Current.Value ? Color4Extensions.FromHex(@"2e466b").Darken(0.2f) : Color4Extensions.FromHex(@"17191c");
            }

            nubContainer.FadeColour(switchColour, 250, Easing.OutQuint);
            content.TransformTo(nameof(BorderColour), borderColour, 250, Easing.OutQuint);
        }
    }
}
