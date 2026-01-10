using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Framework.Screens;
using YouTubePlayerEX.App.Extensions;
using YouTubePlayerEX.App.Localisation;
using YouTubePlayerEX.App.Screens;

namespace YouTubePlayerEX.App
{
    [Cached(typeof(YouTubePlayerEXApp))]
    public partial class YouTubePlayerEXApp : YouTubePlayerEXAppBase
    {
        private ScreenStack screenStack;

        public static FontUsage DefaultFont = FontUsage.Default.With("Pretendard", 16, "Regular");
        public static FontUsage DefaultFontSDF = FontUsage.Default.With("Pretendard", 16, "Regular");

        [BackgroundDependencyLoader]
        private void load()
        {
            var languages = Enum.GetValues<Language>();

            var mappings = languages.Select(language =>
            {
                string cultureCode = language.ToCultureCode();

                try
                {
                    return new LocaleMapping(new ResourceManagerLocalisationStore(cultureCode));
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"Could not load localisations for language \"{cultureCode}\"");
                    return null;
                }
            }).Where(m => m != null);

            Localisation.AddLocaleMappings(mappings);

            // Add your top-level game components here.
            // A screen stack and sample screen has been provided for convenience, but you can replace it if you don't want to use screens.
            AddRange(new Drawable[]
            {
                screenStack = new ScreenStack
                {
                    RelativeSizeAxes = Axes.Both
                },
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            screenStack.Push(new Loader());
        }
    }
}
