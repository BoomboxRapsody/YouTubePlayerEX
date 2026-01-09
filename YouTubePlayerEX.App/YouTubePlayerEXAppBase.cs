using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.IO.Stores;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osuTK;
using YoutubeExplode;
using YouTubePlayerEX.App.Extensions;
using YouTubePlayerEX.App.Graphics;
using YouTubePlayerEX.App.Input.Binding;
using YouTubePlayerEX.App.Localisation;
using YouTubePlayerEX.App.Online;
using YouTubePlayerEX.Resources;

namespace YouTubePlayerEX.App
{
    [Cached]
    public partial class YouTubePlayerEXAppBase : osu.Framework.Game
    {
        // Anything in this class is shared between the test browser and the game implementation.
        // It allows for caching global dependencies that should be accessible to tests, or changing
        // the screen scaling for all components including the test browser and framework overlays.

        protected override Container<Drawable> Content { get; }

        [Cached]
        public readonly YoutubeClient YouTubeClient = new YoutubeClient();

        [Cached]
        protected readonly YouTubeAPI YouTubeService = new YouTubeAPI();

        /// <summary>
        /// The language in which the app is currently displayed in.
        /// </summary>
        public Bindable<Language> CurrentLanguage { get; } = new Bindable<Language>();

        private Bindable<string> frameworkLocale = null!;

        private IBindable<LocalisationParameters> localisationParameters = null!;

        protected YouTubePlayerEXAppBase()
        {
            GlobalActionContainer globalBindings;

            // Ensure game and tests scale with window size and screen DPI.
            base.Content.Add(globalBindings = new GlobalActionContainer(this)
            {
                Child = Content = new DrawSizePreservingFillContainer
                {
                    // You may want to change TargetDrawSize to your "default" resolution, which will decide how things scale and position when using absolute coordinates.
                    TargetDrawSize = new Vector2(1366, 768)
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager frameworkConfig)
        {
            //Logger.Log(Host.CacheStorage.GetStorageForDirectory("videos").GetFullPath("videoId") + @"\video.mp4");
            Resources.AddStore(new DllResourceStore(typeof(YouTubePlayerEXResources).Assembly));

            frameworkLocale = frameworkConfig.GetBindable<string>(FrameworkSetting.Locale);
            frameworkLocale.BindValueChanged(_ => updateLanguage());

            localisationParameters = Localisation.CurrentParameters.GetBoundCopy();
            localisationParameters.BindValueChanged(_ => updateLanguage(), true);

            CurrentLanguage.BindValueChanged(val => frameworkLocale.Value = val.NewValue.ToCultureCode());

            InitialiseFonts();
        }

        private void updateLanguage() => CurrentLanguage.Value = LanguageExtensions.GetLanguageFor(frameworkLocale.Value, localisationParameters.Value);

        protected virtual void InitialiseFonts()
        {
            AddFont(Resources, @"Fonts/Pretendard/Pretendard-Regular");
            AddFont(Resources, @"Fonts/Pretendard/Pretendard-RegularItalic");
            AddFont(Resources, @"Fonts/Pretendard/Pretendard-Medium");
            AddFont(Resources, @"Fonts/Pretendard/Pretendard-MediumItalic");
            AddFont(Resources, @"Fonts/Pretendard/Pretendard-Light");
            AddFont(Resources, @"Fonts/Pretendard/Pretendard-LightItalic");
            AddFont(Resources, @"Fonts/Pretendard/Pretendard-SemiBold");
            AddFont(Resources, @"Fonts/Pretendard/Pretendard-SemiBoldItalic");
            AddFont(Resources, @"Fonts/Pretendard/Pretendard-Bold");
            AddFont(Resources, @"Fonts/Pretendard/Pretendard-BoldItalic");
            AddFont(Resources, @"Fonts/Pretendard/Pretendard-Black");
            AddFont(Resources, @"Fonts/Pretendard/Pretendard-BlackItalic");

            AddFont(Resources, @"Fonts/NotoSansKR/NotoSansKR-Regular");
            AddFont(Resources, @"Fonts/NotoSansKR/NotoSansKR-RegularItalic");
            AddFont(Resources, @"Fonts/NotoSansKR/NotoSansKR-Medium");
            AddFont(Resources, @"Fonts/NotoSansKR/NotoSansKR-MediumItalic");
            AddFont(Resources, @"Fonts/NotoSansKR/NotoSansKR-Light");
            AddFont(Resources, @"Fonts/NotoSansKR/NotoSansKR-LightItalic");
            AddFont(Resources, @"Fonts/NotoSansKR/NotoSansKR-SemiBold");
            AddFont(Resources, @"Fonts/NotoSansKR/NotoSansKR-SemiBoldItalic");
            AddFont(Resources, @"Fonts/NotoSansKR/NotoSansKR-Bold");
            AddFont(Resources, @"Fonts/NotoSansKR/NotoSansKR-BoldItalic");
            AddFont(Resources, @"Fonts/NotoSansKR/NotoSansKR-Black");
            AddFont(Resources, @"Fonts/NotoSansKR/NotoSansKR-BlackItalic");

            AddFont(Resources, @"Fonts/Noto/Noto-Basic");
            AddFont(Resources, @"Fonts/Noto/Noto-Bopomofo");
            AddFont(Resources, @"Fonts/Noto/Noto-CJK-Basic");
            AddFont(Resources, @"Fonts/Noto/Noto-CJK-Compatibility");
            AddFont(Resources, @"Fonts/Noto/Noto-Hangul");
            AddFont(Resources, @"Fonts/Noto/Noto-Thai");

            Fonts.AddStore(new EmojiStore(Host.Renderer, Resources));
        }
    }
}
