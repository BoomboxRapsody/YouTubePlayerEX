// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using YouTubePlayerEX.App.Graphics.Sprites;
using osuTK;
using YouTubePlayerEX.App.Graphics;
using osu.Framework.Allocation;

namespace YouTubePlayerEX.App.Overlays.OSD
{
    public abstract partial class Toast : Container
    {
        /// <summary>
        /// Extra text to be shown at the bottom of the toast. Usually a key binding if available.
        /// </summary>
        public LocalisableString ExtraText
        {
            get => extraText.Text;
            set => extraText.Text = value.ToUpper();
        }

        private const int toast_minimum_width = 240;

        private readonly Container content;
        private readonly Box background;

        protected override Container<Drawable> Content => content;

        protected readonly AdaptiveSpriteText ValueSpriteText;
        private readonly AdaptiveSpriteText extraText;

        [Resolved]
        private OverlayColourProvider overlayColourProvider { get; set; } = null!;

        protected Toast(LocalisableString description, LocalisableString value)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            // A toast's height is decided (and transformed) by the containing OnScreenDisplay.
            RelativeSizeAxes = Axes.Y;
            AutoSizeAxes = Axes.X;

            InternalChildren = new Drawable[]
            {
                new Container // this container exists just to set a minimum width for the toast
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = toast_minimum_width
                },
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.7f
                },
                content = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                },
                new AdaptiveSpriteText
                {
                    Padding = new MarginPadding(10),
                    Name = "Description",
                    Font = YouTubePlayerEXApp.DefaultFont.With(size: 14, weight: "Bold"),
                    Spacing = new Vector2(1, 0),
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = description.ToUpper()
                },
                ValueSpriteText = new AdaptiveSpriteText
                {
                    Font = YouTubePlayerEXApp.DefaultFont.With(size: 24, weight: "Light"),
                    Padding = new MarginPadding { Horizontal = 10 },
                    Name = "Value",
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Text = value
                },
                extraText = new AdaptiveSpriteText
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Name = "Extra Text",
                    Margin = new MarginPadding { Bottom = 15, Horizontal = 10 },
                    Font = YouTubePlayerEXApp.DefaultFont.With(size: 12, weight: "Bold"),
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            background.Colour = overlayColourProvider.Background5;
            extraText.Colour = overlayColourProvider.Dark1;
        }
    }
}
