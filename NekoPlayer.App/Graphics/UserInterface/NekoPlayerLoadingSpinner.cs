// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Video;
using osuTK;

namespace NekoPlayer.App.Graphics.UserInterface
{
    /// <summary>
    /// A loading spinner.
    /// </summary>
    public partial class NekoPlayerLoadingSpinner : VisibilityContainer
    {
        private readonly Video spinner;

        protected override bool StartHidden => true;

        protected CircularContainer MainContents;

        public const float TRANSITION_DURATION = 500;

        private const float spin_duration = 900;

        /// <summary>
        /// Constuct a new loading spinner.
        /// </summary>
        public NekoPlayerLoadingSpinner()
        {
            Size = new Vector2(70);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Child = MainContents = new CircularContainer
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Children = new Drawable[]
                {
                    spinner = new Video(Directory.GetCurrentDirectory() + "/material3expressive_loadingindicator.mp4", false)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Loop = true,
                        AlwaysPresent = true,
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider overlayColourProvider)
        {
            spinner.Colour = overlayColourProvider.Content2;
            spinner.IsPlaying = true;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
        }

        protected override void Dispose(bool isDisposing)
        {
            spinner.Dispose();
            base.Dispose(isDisposing);
        }

        protected override void PopIn()
        {
            MainContents.ScaleTo(1, TRANSITION_DURATION, Easing.OutQuint);
            this.FadeIn(TRANSITION_DURATION * 2, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            MainContents.ScaleTo(0.8f, TRANSITION_DURATION / 2, Easing.In);
            this.FadeOut(TRANSITION_DURATION, Easing.OutQuint);
        }
    }
}
