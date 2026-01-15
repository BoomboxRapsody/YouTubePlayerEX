using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Development;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.IO.Stores;
using osu.Framework.Localisation;   
using osu.Framework.Logging;
using osu.Framework.Platform;
using osuTK;
using YoutubeExplode;
using YouTubePlayerEX.App.Config;
using YouTubePlayerEX.App.Extensions;
using YouTubePlayerEX.App.Graphics;
using YouTubePlayerEX.App.Graphics.Cursor;
using YouTubePlayerEX.App.Graphics.UserInterface;
using YouTubePlayerEX.App.Input.Binding;
using YouTubePlayerEX.App.Localisation;
using YouTubePlayerEX.App.Online;
using YouTubePlayerEX.App.Updater;
using YouTubePlayerEX.Resources;

namespace YouTubePlayerEX.App
{
    [Cached(typeof(YouTubePlayerEXAppBase))]
    public partial class YouTubePlayerEXAppBase : osu.Framework.Game
    {
        // Anything in this class is shared between the test browser and the game implementation.
        // It allows for caching global dependencies that should be accessible to tests, or changing
        // the screen scaling for all components including the test browser and framework overlays.

        protected override Container<Drawable> Content => content;

        private Container content;

        [Cached]
        public readonly YoutubeClient YouTubeClient = new YoutubeClient();

        protected YouTubeAPI YouTubeService { get; set; }

        protected GoogleTranslate TranslateAPI { get; set; }

        /// <summary>
        /// The language in which the app is currently displayed in.
        /// </summary>
        public Bindable<Language> CurrentLanguage { get; } = new Bindable<Language>();

        private Bindable<string> frameworkLocale = null!;

        private IBindable<LocalisationParameters> localisationParameters = null!;

        protected YTPlayerEXConfigManager LocalConfig { get; private set; }

        protected GlobalCursorDisplay GlobalCursorDisplay { get; private set; }

        public Bindable<LocalisableString> UpdateManagerVersionText = new Bindable<LocalisableString>();
        public Bindable<bool> RestartRequired = new Bindable<bool>();

        protected YouTubePlayerEXAppBase()
        {
        }

        protected Storage Storage { get; set; }

        private int allowableExceptions;

        /// <summary>
        /// Allows a maximum of one unhandled exception, per second of execution.
        /// </summary>
        /// <returns>Whether to ignore the exception and continue running.</returns>
        private bool onExceptionThrown(Exception ex)
        {
            if (Interlocked.Decrement(ref allowableExceptions) < 0)
            {
                Logger.Log("Too many unhandled exceptions, crashing out.");
                return false;
            }

            Logger.Log($"Unhandled exception has been allowed with {allowableExceptions} more allowable exceptions.");
            // restore the stock of allowable exceptions after a short delay.
            Task.Delay(1000).ContinueWith(_ => Interlocked.Increment(ref allowableExceptions));

            return true;
        }

        public override void SetHost(GameHost host)
        {
            base.SetHost(host);

            // may be non-null for certain tests
            Storage ??= host.Storage;

            if (host.Window != null)
            {
                host.Window.CursorState |= CursorState.Hidden;
            }

            LocalConfig ??= new YTPlayerEXConfigManager(Storage);

            host.ExceptionThrown += onExceptionThrown;
        }

        public string ParseVideoQuality()
        {
            VideoQuality videoQuality = LocalConfig.Get<VideoQuality>(YTPlayerEXSetting.VideoQuality);

            switch (videoQuality)
            {
                case VideoQuality.Quality_4K:
                    return "2160p";
                case VideoQuality.Quality_1440p:
                    return "1440p";
                case VideoQuality.Quality_1080p:
                    return "1080p";
                case VideoQuality.Quality_720p:
                    return "720p";
                case VideoQuality.Quality_480p:
                    return "480p";
                case VideoQuality.Quality_360p:
                    return "360p";
                case VideoQuality.Quality_240p:
                    return "240p";
                case VideoQuality.Quality_144p:
                    return "144p";
            }

            return string.Empty;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            LocalConfig?.Dispose();

            if (Host != null)
                Host.ExceptionThrown -= onExceptionThrown;
        }

        protected SessionStatics SessionStatics { get; private set; }

        public virtual Version AssemblyVersion => Assembly.GetEntryAssembly()?.GetName().Version ?? new Version();

        /// <summary>
        /// MD5 representation of the game executable.
        /// </summary>
        public string VersionHash { get; private set; }

        public bool IsDeployedBuild => AssemblyVersion.Major > 0;

        public virtual string Version
        {
            get
            {
                if (!IsDeployedBuild)
                    return @"local " + (DebugUtils.IsDebugBuild ? @"debug" : @"release");

                string informationalVersion = Assembly.GetEntryAssembly()?
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                    .InformationalVersion;

                if (!string.IsNullOrEmpty(informationalVersion))
                    return informationalVersion.Split('+').First();

                Version version = AssemblyVersion;
                return $@"{version.Major}.{version.Minor}.{version.Build}";
            }
        }

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager frameworkConfig)
        {
            RestartRequired.Value = false;
            UpdateManagerVersionText.Value = Version;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            //Logger.Log(Host.CacheStorage.GetStorageForDirectory("videos").GetFullPath("videoId") + @"\video.mp4");
            Resources.AddStore(new DllResourceStore(typeof(YouTubePlayerEXResources).Assembly));

            frameworkLocale = frameworkConfig.GetBindable<string>(FrameworkSetting.Locale);
            frameworkLocale.BindValueChanged(_ => updateLanguage());

            localisationParameters = Localisation.CurrentParameters.GetBoundCopy();
            localisationParameters.BindValueChanged(_ => updateLanguage(), true);

            CurrentLanguage.BindValueChanged(val => frameworkLocale.Value = val.NewValue.ToCultureCode());

            InitialiseFonts();

            dependencies.Cache(LocalConfig);

            dependencies.Cache(TranslateAPI = new GoogleTranslate(this, frameworkConfig));
            dependencies.Cache(YouTubeService = new YouTubeAPI(frameworkConfig, TranslateAPI, LocalConfig));

            dependencies.Cache(SessionStatics = new SessionStatics());

            GlobalActionContainer globalBindings;

            AdaptiveMenuSamples menuSamples;
            dependencies.Cache(menuSamples = new AdaptiveMenuSamples());
            base.Content.Add(menuSamples);

            // Ensure game and tests scale with window size and screen DPI.
            base.Content.Add(globalBindings = new GlobalActionContainer(this)
            {
                Child = new DrawSizePreservingFillContainer
                {
                    // You may want to change TargetDrawSize to your "default" resolution, which will decide how things scale and position when using absolute coordinates.
                    TargetDrawSize = new Vector2(1366, 768),
                    Children = new Drawable[]
                    {
                        (GlobalCursorDisplay = new GlobalCursorDisplay
                        {
                            RelativeSizeAxes = Axes.Both
                        }).WithChild(content = new AdaptiveTooltipContainer(GlobalCursorDisplay.MenuCursor)
                        {
                            RelativeSizeAxes = Axes.Both
                        }),
                    }
                }
            });
        }

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        public void RequestUpdateWindowTitle(string customTitle)
        {
            if (Host.Window == null)
                return;

            string newTitle = "YouTube Player EX";

            if (!string.IsNullOrEmpty(customTitle))
            {
                newTitle = $"YouTube Player EX - {customTitle}";
            }

            if (newTitle != Host.Window.Title)
                Host.Window.Title = newTitle;
        }

        private void updateLanguage() => CurrentLanguage.Value = LanguageExtensions.GetLanguageFor(frameworkLocale.Value, localisationParameters.Value);

        protected virtual void InitialiseFonts()
        {
            AddFont(Resources, @"Fonts/UIFonts/Pretendard/Pretendard-Regular");
            AddFont(Resources, @"Fonts/UIFonts/Pretendard/Pretendard-RegularItalic");
            AddFont(Resources, @"Fonts/UIFonts/Pretendard/Pretendard-Medium");
            AddFont(Resources, @"Fonts/UIFonts/Pretendard/Pretendard-MediumItalic");
            AddFont(Resources, @"Fonts/UIFonts/Pretendard/Pretendard-Light");
            AddFont(Resources, @"Fonts/UIFonts/Pretendard/Pretendard-LightItalic");
            AddFont(Resources, @"Fonts/UIFonts/Pretendard/Pretendard-SemiBold");
            AddFont(Resources, @"Fonts/UIFonts/Pretendard/Pretendard-SemiBoldItalic");
            AddFont(Resources, @"Fonts/UIFonts/Pretendard/Pretendard-Bold");
            AddFont(Resources, @"Fonts/UIFonts/Pretendard/Pretendard-BoldItalic");
            AddFont(Resources, @"Fonts/UIFonts/Pretendard/Pretendard-Black");
            AddFont(Resources, @"Fonts/UIFonts/Pretendard/Pretendard-BlackItalic");

            AddFont(Resources, @"Fonts/UIFonts/NotoSansKR/NotoSansKR-Regular");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansKR/NotoSansKR-RegularItalic");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansKR/NotoSansKR-Medium");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansKR/NotoSansKR-MediumItalic");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansKR/NotoSansKR-Light");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansKR/NotoSansKR-LightItalic");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansKR/NotoSansKR-SemiBold");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansKR/NotoSansKR-SemiBoldItalic");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansKR/NotoSansKR-Bold");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansKR/NotoSansKR-BoldItalic");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansKR/NotoSansKR-Black");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansKR/NotoSansKR-BlackItalic");

            AddFont(Resources, @"Fonts/UIFonts/NotoSansSyriac/NotoSansSyriac-Regular");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansSyriac/NotoSansSyriac-RegularItalic");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansSyriac/NotoSansSyriac-Medium");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansSyriac/NotoSansSyriac-MediumItalic");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansSyriac/NotoSansSyriac-Light");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansSyriac/NotoSansSyriac-LightItalic");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansSyriac/NotoSansSyriac-SemiBold");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansSyriac/NotoSansSyriac-SemiBoldItalic");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansSyriac/NotoSansSyriac-Bold");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansSyriac/NotoSansSyriac-BoldItalic");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansSyriac/NotoSansSyriac-Black");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansSyriac/NotoSansSyriac-BlackItalic");

            AddFont(Resources, @"Fonts/UIFonts/NotoSansOriya/NotoSansOriya-Regular");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansOriya/NotoSansOriya-RegularItalic");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansOriya/NotoSansOriya-Medium");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansOriya/NotoSansOriya-MediumItalic");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansOriya/NotoSansOriya-Light");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansOriya/NotoSansOriya-LightItalic");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansOriya/NotoSansOriya-SemiBold");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansOriya/NotoSansOriya-SemiBoldItalic");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansOriya/NotoSansOriya-Bold");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansOriya/NotoSansOriya-BoldItalic");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansOriya/NotoSansOriya-Black");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansOriya/NotoSansOriya-BlackItalic");

            AddFont(Resources, @"Fonts/UIFonts/NotoSansSymbols/NotoSansSymbols-Regular");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansSymbols/NotoSansSymbols-RegularItalic");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansSymbols/NotoSansSymbols-Medium");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansSymbols/NotoSansSymbols-MediumItalic");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansSymbols/NotoSansSymbols-Light");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansSymbols/NotoSansSymbols-LightItalic");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansSymbols/NotoSansSymbols-SemiBold");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansSymbols/NotoSansSymbols-SemiBoldItalic");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansSymbols/NotoSansSymbols-Bold");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansSymbols/NotoSansSymbols-BoldItalic");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansSymbols/NotoSansSymbols-Black");
            AddFont(Resources, @"Fonts/UIFonts/NotoSansSymbols/NotoSansSymbols-BlackItalic");

            AddFont(Resources, @"Fonts/UIFonts/NotoSans/NotoSans-Regular");
            AddFont(Resources, @"Fonts/UIFonts/NotoSans/NotoSans-RegularItalic");
            AddFont(Resources, @"Fonts/UIFonts/NotoSans/NotoSans-Medium");
            AddFont(Resources, @"Fonts/UIFonts/NotoSans/NotoSans-MediumItalic");
            AddFont(Resources, @"Fonts/UIFonts/NotoSans/NotoSans-Light");
            AddFont(Resources, @"Fonts/UIFonts/NotoSans/NotoSans-LightItalic");
            AddFont(Resources, @"Fonts/UIFonts/NotoSans/NotoSans-SemiBold");
            AddFont(Resources, @"Fonts/UIFonts/NotoSans/NotoSans-SemiBoldItalic");
            AddFont(Resources, @"Fonts/UIFonts/NotoSans/NotoSans-Bold");
            AddFont(Resources, @"Fonts/UIFonts/NotoSans/NotoSans-BoldItalic");
            AddFont(Resources, @"Fonts/UIFonts/NotoSans/NotoSans-Black");
            AddFont(Resources, @"Fonts/UIFonts/NotoSans/NotoSans-BlackItalic");

            AddFont(Resources, @"Fonts/UIFonts/Noto/Noto-Basic");
            AddFont(Resources, @"Fonts/UIFonts/Noto/Noto-Bopomofo");
            AddFont(Resources, @"Fonts/UIFonts/Noto/Noto-CJK-Basic");
            AddFont(Resources, @"Fonts/UIFonts/Noto/Noto-CJK-Compatibility");
            AddFont(Resources, @"Fonts/UIFonts/Noto/Noto-Hangul");
            AddFont(Resources, @"Fonts/UIFonts/Noto/Noto-Thai");

            Fonts.AddStore(new EmojiStore(Host.Renderer, Resources));
        }
    }
}
