// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using YouTubePlayerEX.App.Graphics.Videos;

namespace YouTubePlayerEX.App.Screens
{
    public partial class IntroScreen : YouTubePlayerEXScreen
    {
        private IntroScreenVideo intro;

        public IntroScreen()
        {
            ValidForResume = false;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(intro = new IntroScreenVideo() { RelativeSizeAxes = Axes.Both });

            intro.TrackCompleted = () =>
            {
                Schedule(() => this.Push(new MainAppView()));
            };
        }
    }
}
