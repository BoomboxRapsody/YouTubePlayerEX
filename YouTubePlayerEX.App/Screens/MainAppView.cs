// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Humanizer;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Video;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Platform.Windows;
using osu.Framework.Statistics;
using osu.Framework.Threading;
using osuTK;
using osuTK.Graphics;
using SharpCompress.Archives.Zip;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeExplode.Videos.Streams;
using YouTubePlayerEX.App.Config;
using YouTubePlayerEX.App.Extensions;
using YouTubePlayerEX.App.Graphics;
using YouTubePlayerEX.App.Graphics.Containers;
using YouTubePlayerEX.App.Graphics.Shaders;
using YouTubePlayerEX.App.Graphics.Sprites;
using YouTubePlayerEX.App.Graphics.UserInterface;
using YouTubePlayerEX.App.Graphics.UserInterfaceV2;
using YouTubePlayerEX.App.Graphics.Videos;
using YouTubePlayerEX.App.Input;
using YouTubePlayerEX.App.Input.Binding;
using YouTubePlayerEX.App.Localisation;
using YouTubePlayerEX.App.Online;
using YouTubePlayerEX.App.Overlays;
using YouTubePlayerEX.App.Overlays.OSD;
using YouTubePlayerEX.App.Updater;
using YouTubePlayerEX.App.Utils;
using static YouTubePlayerEX.App.YouTubePlayerEXApp;
using Container = osu.Framework.Graphics.Containers.Container;
using Language = YouTubePlayerEX.App.Localisation.Language;
using OverlayContainer = YouTubePlayerEX.App.Graphics.Containers.OverlayContainer;

namespace YouTubePlayerEX.App.Screens
{
    public partial class MainAppView : YouTubePlayerEXScreen, IKeyBindingHandler<GlobalAction>
    {
        private BufferedContainer videoContainer;
        private AdaptiveButton loadBtn, commentSendButton, searchButton;
        private AdaptiveTextBox videoIdBox, commentTextBox, searchTextBox;
        private LoadingSpinner spinner;
        private ScheduledDelegate spinnerShow;
        private AdaptiveAlertContainer alert;
        private IdleTracker idleTracker;
        private Container uiContainer;
        private Container uiGradientContainer;
        private OverlayContainer loadVideoContainer, settingsContainer, videoDescriptionContainer, commentsContainer, videoInfoExpertOverlay, searchContainer, reportAbuseOverlay;
        private AdaptiveButtonWithShadow loadBtnOverlayShow, settingsOverlayShowBtn, commentOpenButton, searchOpenButton, reportOpenButton;
        private VideoMetadataDisplayWithoutProfile videoMetadataDisplay;
        private VideoMetadataDisplay videoMetadataDisplayDetails;
        private RoundedButtonContainer commentOpenButtonDetails, likeButton;

        private LinkFlowContainer madeByText;

        private SettingsItemV2 pixel_shader_size_adjust, audioLanguageItem;

        private Sample overlayShowSample;
        private Sample overlayHideSample;
        private AdaptiveButtonV2 reportButton;
        private FormTextBox reportComment;

        private Container overlayFadeContainer;
        private RoundedButtonContainer dislikeButton;

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            // Be sure to dispose the track, otherwise memory will be leaked!
            // This is automatic for DrawableTrack.
            overlayShowSample.Dispose();
            overlayHideSample.Dispose();
        }

        private AdaptiveSpriteText videoLoadingProgress, videoInfoDetails, likeCount, dislikeCount, commentCount, commentsContainerTitle, currentTime, totalTime;
        private LinkFlowContainer videoDescription, gameVersion;
        private FillFlowContainer commentContainer, searchResultContainer;

        [Resolved]
        private GoogleOAuth2 googleOAuth2 { get; set; } = null!;

        private ReportDropdown reportReason, reportSubReason;

        private BindableNumber<double> videoProgress = new BindableNumber<double>()
        {
            MinValue = 0,
            MaxValue = 1,
        };

        private Bindable<double> windowedPositionX = null!;
        private Bindable<double> windowedPositionY = null!;
        private Bindable<WindowMode> windowMode = null!;
        private Bindable<List<InternalShader>> appliedEffects = new Bindable<List<InternalShader>>();

        private Bindable<ClosedCaptionLanguage> captionLanguage = null!;
        private bool isControlVisible = true;

        private void onDisplaysChanged(IEnumerable<Display> displays)
        {
            Scheduler.AddOnce(d =>
            {
                if (!displayDropdown.Items.SequenceEqual(d, DisplayListComparer.DEFAULT))
                    displayDropdown.Items = d;
                updateDisplaySettingsVisibility();
            }, displays);
        }

        private Bindable<Config.VideoQuality> videoQuality;
        private Bindable<HardwareVideoDecoder> hardwareVideoDecoder;
        private Bindable<Localisation.Language> audioLanguage;
        private Bindable<bool> adjustPitch;
        private Bindable<string> localeBindable = new Bindable<string>();
        private FormButton checkForUpdatesButton, login;
        private ThumbnailContainerBackground thumbnailContainer;
        private AdaptiveSliderBar<double> seekbar;
        private Bindable<LocalisableString> updateInfomationText;
        private Bindable<bool> updateButtonEnabled, fpsDisplay;
        private Bindable<AspectRatioMethod> aspectRatioMethod;

        private BufferedContainer videoScalingContainer;

        private Box likeButtonBackground, dislikeButtonBackground, likeButtonBackgroundSelected, dislikeButtonBackgroundSelected;
        private FillFlowContainer likeButtonForeground, dislikeButtonForeground;

        private Container userInterfaceContainer;

        private Bindable<bool> alwaysUseOriginalAudio;

        [Resolved]
        private AdaptiveColour colours { get; set; } = null!;

        private Bindable<SettingsNote.Data> videoQualityWarning = new Bindable<SettingsNote.Data>();

        private Bindable<float> scalingBackgroundDim = null!;

        private LinkFlowContainer dislikeCounterCredits;

        private Bindable<bool> signedIn;

        [Resolved]
        private ShaderManager shaderManager { get; set; } = null!;

        private FormCheckBox crt_shader, pixel_shader = null!;

        private PixelShader pixelShader = null!;
        private CrtShader crtShader = null!;
        private Bindable<float> pixelShaderSize = new BindableFloat(5)
        {
            MaxValue = 30,
            MinValue = 2,
        };
        private Bindable<Color4> crtBackgroundBindable = new Bindable<Color4>(Color4.Black);

        protected T GetShaderByType<T>() where T : InternalShader, new()
            => shaderManager.LocalInternalShader<T>();

        [BackgroundDependencyLoader]
        private void load(ISampleStore sampleStore, FrameworkConfigManager config, YTPlayerEXConfigManager appConfig, GameHost host, Storage storage, OverlayColourProvider overlayColourProvider, TextureStore textures)
        {
            appliedEffects.Value = new List<InternalShader>();
            window = host.Window;

            pixelShader = GetShaderByType<PixelShader>();
            crtShader = GetShaderByType<CrtShader>();

            uiVisible = screenshotManager.CursorVisibility.GetBoundCopy();
            signedIn = googleOAuth2.SignedIn.GetBoundCopy();

            isAnyOverlayOpen = sessionStatics.GetBindable<bool>(Static.IsAnyOverlayOpen);
            videoPlaying = sessionStatics.GetBindable<bool>(Static.IsVideoPlaying);

            usernameDisplayMode = appConfig.GetBindable<UsernameDisplayMode>(YTPlayerEXSetting.UsernameDisplayMode);

            var renderer = config.GetBindable<RendererType>(FrameworkSetting.Renderer);
            automaticRendererInUse = renderer.Value == RendererType.Automatic;

            scalingMode = appConfig.GetBindable<ScalingMode>(YTPlayerEXSetting.Scaling);
            scalingSizeX = appConfig.GetBindable<float>(YTPlayerEXSetting.ScalingSizeX);
            scalingSizeY = appConfig.GetBindable<float>(YTPlayerEXSetting.ScalingSizeY);
            scalingPositionX = appConfig.GetBindable<float>(YTPlayerEXSetting.ScalingPositionX);
            scalingPositionY = appConfig.GetBindable<float>(YTPlayerEXSetting.ScalingPositionY);
            scalingBackgroundDim = appConfig.GetBindable<float>(YTPlayerEXSetting.ScalingBackgroundDim);
            alwaysUseOriginalAudio = appConfig.GetBindable<bool>(YTPlayerEXSetting.AlwaysUseOriginalAudio);

            exportStorage = storage.GetStorageForDirectory(@"exports");

            localeBindable = config.GetBindable<string>(FrameworkSetting.Locale);
            fpsDisplay = appConfig.GetBindable<bool>(YTPlayerEXSetting.ShowFpsDisplay);
            adjustPitch = appConfig.GetBindable<bool>(YTPlayerEXSetting.AdjustPitchOnSpeedChange);
            videoQuality = appConfig.GetBindable<Config.VideoQuality>(YTPlayerEXSetting.VideoQuality);
            audioLanguage = appConfig.GetBindable<Localisation.Language>(YTPlayerEXSetting.AudioLanguage);
            hardwareVideoDecoder = config.GetBindable<HardwareVideoDecoder>(FrameworkSetting.HardwareVideoDecoder);
            cursorInWindow = host.Window?.CursorInWindow.GetBoundCopy();
            windowMode = config.GetBindable<WindowMode>(FrameworkSetting.WindowMode);
            captionLanguage = appConfig.GetBindable<ClosedCaptionLanguage>(YTPlayerEXSetting.ClosedCaptionLanguage);
            sizeFullscreen = config.GetBindable<Size>(FrameworkSetting.SizeFullscreen);
            sizeWindowed = config.GetBindable<Size>(FrameworkSetting.WindowedSize);
            windowedPositionX = config.GetBindable<double>(FrameworkSetting.WindowedPositionX);
            windowedPositionY = config.GetBindable<double>(FrameworkSetting.WindowedPositionY);
            updateInfomationText = game.UpdateManagerVersionText.GetBoundCopy();
            updateButtonEnabled = game.UpdateButtonEnabled.GetBoundCopy();

            aspectRatioMethod = appConfig.GetBindable<AspectRatioMethod>(YTPlayerEXSetting.AspectRatioMethod);

            windowedResolution.Value = sizeWindowed.Value;

            appliedEffects.BindValueChanged(shaders =>
            {
                currentVideoSource?.ApplyShaders(shaders.NewValue);
            }, true);

            if (window != null)
            {
                currentDisplay.BindTo(window.CurrentDisplayBindable);
                window.DisplaysChanged += onDisplaysChanged;
            }

            if (host.Renderer is IWindowsRenderer windowsRenderer)
                fullscreenCapability.BindTo(windowsRenderer.FullscreenCapability);

            overlayShowSample = sampleStore.Get(@"overlay-pop-in");
            overlayHideSample = sampleStore.Get(@"overlay-pop-out");
            InternalChildren = new Drawable[]
            {
                idleTracker = new AppIdleTracker(3000),
                videoScalingContainer = new BufferedContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new ScalingContainerNew(ScalingMode.Video)
                    {
                        Children = new Drawable[] {
                            new ParallaxContainer
                            {
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = Color4.Black,
                                    },
                                    thumbnailContainer = new ThumbnailContainerBackground
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                    },
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = Color4.Black,
                                        Alpha = .5f,
                                    },
                                },
                            },
                        },
                    },
                },
                videoContainer = new BufferedContainer
                {
                    RelativeSizeAxes = Axes.Both,
                },
                userInterfaceContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        spinner = new LoadingSpinner(true, true)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Margin = new MarginPadding(40),
                        },
                        videoLoadingProgress = new AdaptiveSpriteText
                        {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Margin = new MarginPadding
                            {
                                Bottom = 110,
                            },
                        },
                        uiGradientContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0.35f), Color4.Black.Opacity(0)),
                                    Origin = Anchor.TopLeft,
                                    Anchor = Anchor.TopLeft,
                                    RelativeSizeAxes = Axes.X,
                                    Height = 300,
                                },
                                new Box
                                {
                                    Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0), Color4.Black.Opacity(0.35f)),
                                    Origin = Anchor.BottomLeft,
                                    Anchor = Anchor.BottomLeft,
                                    RelativeSizeAxes = Axes.X,
                                    Height = 300,
                                },
                            }
                        },
                        uiContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding(8),
                            Children = new Drawable[]
                            {
                                videoMetadataDisplay = new VideoMetadataDisplayWithoutProfile
                                {
                                    Width = 400,
                                    Height = 60,
                                    Origin = Anchor.TopLeft,
                                    Anchor = Anchor.TopLeft,
                                    ClickEvent = _ => showOverlayContainer(videoDescriptionContainer),
                                },
                                loadBtnOverlayShow = new IconButtonWithShadow
                                {
                                    Enabled = { Value = true },
                                    Origin = Anchor.TopRight,
                                    Anchor = Anchor.TopRight,
                                    Size = new Vector2(40, 40),
                                    Icon = FontAwesome.Regular.FolderOpen,
                                    IconScale = new Vector2(1.2f),
                                    TooltipText = YTPlayerEXStrings.LoadVideo,
                                },
                                settingsOverlayShowBtn = new IconButtonWithShadow
                                {
                                    Enabled = { Value = true },
                                    Margin = new MarginPadding
                                    {
                                        Right = 48,
                                    },
                                    Origin = Anchor.TopRight,
                                    Anchor = Anchor.TopRight,
                                    Size = new Vector2(40, 40),
                                    Icon = FontAwesome.Solid.Cog,
                                    IconScale = new Vector2(1.2f),
                                    TooltipText = YTPlayerEXStrings.Settings,
                                },
                                commentOpenButton = new IconButtonWithShadow
                                {
                                    Enabled = { Value = false },
                                    Margin = new MarginPadding
                                    {
                                        Right = 96,
                                    },
                                    Origin = Anchor.TopRight,
                                    Anchor = Anchor.TopRight,
                                    Size = new Vector2(40, 40),
                                    Icon = FontAwesome.Regular.CommentAlt,
                                    IconScale = new Vector2(1.2f),
                                    TooltipText = YTPlayerEXStrings.CommentsWithoutCount,
                                },
                                searchOpenButton = new IconButtonWithShadow
                                {
                                    Enabled = { Value = true },
                                    Margin = new MarginPadding
                                    {
                                        Right = 144,
                                    },
                                    Origin = Anchor.TopRight,
                                    Anchor = Anchor.TopRight,
                                    Size = new Vector2(40, 40),
                                    Icon = FontAwesome.Solid.Search,
                                    IconScale = new Vector2(1.2f),
                                    TooltipText = YTPlayerEXStrings.Search,
                                },
                                reportOpenButton = new IconButtonWithShadow
                                {
                                    Enabled = { Value = false },
                                    Margin = new MarginPadding
                                    {
                                        Right = 192,
                                    },
                                    Origin = Anchor.TopRight,
                                    Anchor = Anchor.TopRight,
                                    Size = new Vector2(40, 40),
                                    Icon = FontAwesome.Solid.Flag,
                                    IconScale = new Vector2(1.2f),
                                    TooltipText = YTPlayerEXStrings.Report,
                                },
                                new Container {
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomCentre,
                                    RelativeSizeAxes = Axes.X,
                                    Height = 100,
                                    Masking = true,
                                    CornerRadius = YouTubePlayerEXApp.UI_CORNER_RADIUS,
                                    EdgeEffect = new osu.Framework.Graphics.Effects.EdgeEffectParameters
                                    {
                                        Type = osu.Framework.Graphics.Effects.EdgeEffectType.Shadow,
                                        Colour = Color4.Black.Opacity(0.25f),
                                        Offset = new Vector2(0, 2),
                                        Radius = 16,
                                    },
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = overlayColourProvider.Background5,
                                            Alpha = 1f,
                                        },
                                        new FillFlowContainer {
                                            RelativeSizeAxes = Axes.Both,
                                            Padding = new MarginPadding(16),
                                            Spacing = new Vector2(0, 8),
                                            Children = new Drawable[] {
                                                seekbar = new RoundedSliderBarWithoutTooltip
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    PlaySamplesOnAdjust = false,
                                                    DisplayAsPercentage = true,
                                                    Current = { BindTarget = videoProgress },
                                                },
                                                new Container
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    AutoSizeAxes = Axes.Y,
                                                    Children = new Drawable[] {
                                                        currentTime = new AdaptiveSpriteText
                                                        {
                                                            Anchor = Anchor.TopLeft,
                                                            Origin = Anchor.TopLeft,
                                                            Text = "0:00",
                                                            Colour = overlayColourProvider.Content2,
                                                        },
                                                        totalTime = new AdaptiveSpriteText
                                                        {
                                                            Anchor = Anchor.TopRight,
                                                            Origin = Anchor.TopRight,
                                                            Text = "0:00",
                                                            Colour = overlayColourProvider.Content2,
                                                        }
                                                    },
                                                },
                                                new FillFlowContainer
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    AutoSizeAxes = Axes.Y,
                                                    Spacing = new Vector2(8, 0),
                                                    Children = new Drawable[]
                                                    {
                                                        playPause = new IconButton
                                                        {
                                                            Enabled = { Value = true },
                                                            Icon = FontAwesome.Solid.Play,
                                                            TooltipText = YTPlayerEXStrings.Play,
                                                            IconColour = overlayColourProvider.Content2,
                                                            ClickAction = _ =>
                                                            {
                                                                if (currentVideoSource != null)
                                                                {
                                                                    if (currentVideoSource.IsPlaying())
                                                                        currentVideoSource.Pause();
                                                                    else
                                                                        currentVideoSource.Play();
                                                                }
                                                            }
                                                        },
                                                        new Container
                                                        {
                                                            AutoSizeAxes = Axes.X,
                                                            Height = 30,
                                                            Masking = true,
                                                            CornerRadius = 15,
                                                            Children = new Drawable[]
                                                            {
                                                                new Box
                                                                {
                                                                    RelativeSizeAxes = Axes.Both,
                                                                    Colour = overlayColourProvider.Background3,
                                                                    Alpha = 0.7f,
                                                                },
                                                                new FillFlowContainer
                                                                {
                                                                    AutoSizeAxes = Axes.Both,
                                                                    Spacing = new Vector2(8, 0),
                                                                    Direction = FillDirection.Horizontal,
                                                                    Padding = new MarginPadding
                                                                    {
                                                                        Horizontal = 8
                                                                    },
                                                                    Children = new Drawable[]
                                                                    {
                                                                        new SpriteIcon
                                                                        {
                                                                            Icon = FontAwesome.Solid.TachometerAlt,
                                                                            Width = 16,
                                                                            Height = 16,
                                                                            Margin = new MarginPadding
                                                                            {
                                                                                Top = 8,
                                                                            },
                                                                            Colour = overlayColourProvider.Content2,
                                                                        },
                                                                        new PlaybackSpeedSliderBar
                                                                        {
                                                                            Width = 200,
                                                                            Margin = new MarginPadding
                                                                            {
                                                                                Top = 8,
                                                                            },
                                                                            KeyboardStep = 0.05f,
                                                                            PlaySamplesOnAdjust = true,
                                                                            Current = { BindTarget = playbackSpeed },
                                                                        },
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    },
                                }
                            }
                        },
                        overlayFadeContainer = new OverlayFadeContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            ClickAction = _ => hideOverlays(),
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.Black,
                            }
                        },
                        loadVideoContainer = new OverlayContainer
                        {
                            Width = 400,
                            Height = 200,
                            CornerRadius = YouTubePlayerEXApp.UI_CORNER_RADIUS,
                            Masking = true,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            EdgeEffect = new osu.Framework.Graphics.Effects.EdgeEffectParameters
                            {
                                Type = osu.Framework.Graphics.Effects.EdgeEffectType.Shadow,
                                Colour = Color4.Black.Opacity(0.25f),
                                Offset = new Vector2(0, 2),
                                Radius = 16,
                            },
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = overlayColourProvider.Background5,
                                },
                                new AdaptiveSpriteText
                                {
                                    Origin = Anchor.TopLeft,
                                    Anchor = Anchor.TopLeft,
                                    Text = YTPlayerEXStrings.LoadFromVideoId,
                                    Margin = new MarginPadding(16),
                                    Font = YouTubePlayerEXApp.DefaultFont.With(family: "Torus-Alternate", size: 30, weight: "Bold"),
                                    Colour = overlayColourProvider.Content2,
                                },
                                loadBtn = new AdaptiveButton
                                {
                                    Enabled = { Value = true },
                                    Origin = Anchor.BottomRight,
                                    Anchor = Anchor.BottomRight,
                                    Text = YTPlayerEXStrings.LoadVideo,
                                    Size = new Vector2(200, 60),
                                    Margin = new MarginPadding(8),
                                },
                                videoIdBox = new AdaptiveTextBox
                                {
                                    Origin = Anchor.CentreRight,
                                    Anchor = Anchor.CentreRight,
                                    Text = "",
                                    FontSize = 30,
                                    Size = new Vector2(385, 60),
                                    Margin = new MarginPadding(8),
                                },
                            }
                        },
                        settingsContainer = new OverlayContainer
                        {
                            Size = new Vector2(0.7f),
                            RelativeSizeAxes = Axes.Both,
                            CornerRadius = YouTubePlayerEXApp.UI_CORNER_RADIUS,
                            Masking = true,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            EdgeEffect = new osu.Framework.Graphics.Effects.EdgeEffectParameters
                            {
                                Type = osu.Framework.Graphics.Effects.EdgeEffectType.Shadow,
                                Colour = Color4.Black.Opacity(0.25f),
                                Offset = new Vector2(0, 2),
                                Radius = 16,
                            },
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = overlayColourProvider.Background5,
                                },
                                new AdaptiveSpriteText
                                {
                                    Origin = Anchor.TopLeft,
                                    Anchor = Anchor.TopLeft,
                                    Text = YTPlayerEXStrings.Settings,
                                    Margin = new MarginPadding(16),
                                    Font = YouTubePlayerEXApp.DefaultFont.With(family: "Torus-Alternate", size: 30, weight: "Bold"),
                                    Colour = overlayColourProvider.Content2,
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding
                                    {
                                        Horizontal = 16,
                                        Bottom = 16,
                                        Top = 56,
                                    },
                                    Children = new Drawable[] {
                                        new AdaptiveScrollContainer
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            ScrollbarVisible = false,
                                            Children = new Drawable[]
                                            {
                                                new FillFlowContainer {
                                                    RelativeSizeAxes = Axes.X,
                                                    AutoSizeAxes = Axes.Y,
                                                    Spacing = new Vector2(0, 4),
                                                    Direction = FillDirection.Vertical,
                                                    Children = new Drawable[] {
                                                        new AdaptiveSpriteText
                                                        {
                                                            Font = YouTubePlayerEXApp.DefaultFont.With(family: "Torus-Alternate", size: 30),
                                                            Text = YTPlayerEXStrings.QuickAction,
                                                            Padding = new MarginPadding { Horizontal = 30, Bottom = 12 },
                                                            Colour = overlayColourProvider.Content2,
                                                        },
                                                        new SettingsButtonV2
                                                        {
                                                            Text = YTPlayerEXStrings.ExportLogs,
                                                            Padding = new MarginPadding { Horizontal = 30 },
                                                            BackgroundColour = colours.YellowDarker.Darken(0.5f),
                                                            Action = () => Task.Run(exportLogs),
                                                        },
                                                        new SettingsButtonV2
                                                        {
                                                            Text = @"Clear all caches",
                                                            Padding = new MarginPadding { Horizontal = 30 },
                                                            Action = () =>
                                                            {
                                                                host.Collect();

                                                                // host.Collect() uses GCCollectionMode.Optimized, but we should be as aggressive as possible here.
                                                                GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, true, true);
                                                            }
                                                        },
                                                        new AdaptiveSpriteText
                                                        {
                                                            Font = YouTubePlayerEXApp.DefaultFont.With(family: "Torus-Alternate", size: 30),
                                                            Text = YTPlayerEXStrings.General,
                                                            Padding = new MarginPadding { Horizontal = 30, Vertical = 12 },
                                                            Colour = overlayColourProvider.Content2,
                                                        },
                                                        new SettingsItemV2(new FormEnumDropdown<Language>
                                                        {
                                                            Caption = YTPlayerEXStrings.Language,
                                                            Current = game.CurrentLanguage,
                                                        })
                                                        {
                                                            ShowRevertToDefaultButton = false,
                                                            CanBeShown = { BindTarget = displayDropdownCanBeShown }
                                                        },
                                                        new SettingsItemV2(new FormEnumDropdown<ClosedCaptionLanguage>
                                                        {
                                                            Caption = YTPlayerEXStrings.CaptionLanguage,
                                                            Current = captionLanguage,
                                                            Hotkey = new Hotkey(GlobalAction.CycleCaptionLanguage),
                                                        }),
                                                        new SettingsItemV2(new FormEnumDropdown<VideoMetadataTranslateSource>
                                                        {
                                                            Caption = YTPlayerEXStrings.VideoMetadataTranslateSource,
                                                            Current = appConfig.GetBindable<VideoMetadataTranslateSource>(YTPlayerEXSetting.VideoMetadataTranslateSource),
                                                        }),
                                                        new SettingsItemV2(new FormEnumDropdown<UsernameDisplayMode>
                                                        {
                                                            Caption = YTPlayerEXStrings.UsernameDisplayMode,
                                                            Current = usernameDisplayMode,
                                                        }),
                                                        new SettingsItemV2(login = new FormButton
                                                        {
                                                            Caption = YTPlayerEXStrings.GoogleAccount,
                                                            Text = YTPlayerEXStrings.SignedOut,
                                                            Action = () => {
                                                                if (!googleOAuth2.SignedIn.Value)
                                                                {
                                                                    Task.Run(() => googleOAuth2.SignIn());
                                                                }
                                                                else
                                                                {
                                                                    Task.Run(() => googleOAuth2.SignOut());
                                                                }
                                                            },
                                                        }),
                                                        checkForUpdatesButtonCore = new SettingsItemV2(checkForUpdatesButton = new FormButton
                                                        {
                                                            Caption = YTPlayerEXStrings.CheckUpdate,
                                                            Text = app.Version,
                                                            ButtonIcon = FontAwesome.Solid.Sync,
                                                            Action = () => {
                                                                if (game.UpdateManager is NoActionUpdateManager)
                                                                {
                                                                    host.OpenUrlExternally(@"https://github.com/BoomboxRapsody/YouTubePlayerEX/releases");
                                                                }
                                                                else
                                                                {
                                                                    if (game.RestartRequired.Value != true)
                                                                        checkForUpdates().FireAndForget();
                                                                    else
                                                                        game.RestartAction.Invoke();
                                                                }
                                                            },
                                                        }),
                                                        new AdaptiveSpriteText
                                                        {
                                                            Font = YouTubePlayerEXApp.DefaultFont.With(family: "Torus-Alternate", size: 30),
                                                            Text = YTPlayerEXStrings.Graphics,
                                                            Padding = new MarginPadding { Horizontal = 30, Vertical = 12 },
                                                            Colour = overlayColourProvider.Content2,
                                                        },
                                                        new SettingsItemV2(new FormEnumDropdown<AspectRatioMethod>
                                                        {
                                                            Caption = YTPlayerEXStrings.AspectRatioMethod,
                                                            Current = aspectRatioMethod,
                                                            Hotkey = new Hotkey(GlobalAction.CycleAspectRatio),
                                                        }),
                                                        new SettingsItemV2(new FormSliderBar<double>
                                                        {
                                                            Caption = YTPlayerEXStrings.VideoDimLevel,
                                                            Current = appConfig.GetBindable<double>(YTPlayerEXSetting.VideoDimLevel),
                                                            DisplayAsPercentage = true,
                                                        }),
                                                        new SettingsItemV2(new FormSliderBar<float>
                                                        {
                                                            Caption = YTPlayerEXStrings.UIScaling,
                                                            TransferValueOnCommit = true,
                                                            Current = appConfig.GetBindable<float>(YTPlayerEXSetting.UIScale),
                                                            KeyboardStep = 0.01f,
                                                            LabelFormat = v => $@"{v:0.##}x",
                                                        }),
                                                        new SettingsItemV2(new FrameSyncDropdown
                                                        {
                                                            Caption = YTPlayerEXStrings.FrameLimiter,
                                                            Current = config.GetBindable<FrameSync>(FrameworkSetting.FrameSync),
                                                        }),
                                                        windowModeDropdownSettings = new SettingsItemV2(windowModeDropdown = new WindowModeDropdown
                                                        {
                                                            Caption = YTPlayerEXStrings.ScreenMode,
                                                            Items = window?.SupportedWindowModes,
                                                            Current = windowMode,
                                                        })
                                                        {
                                                            CanBeShown = { Value = window?.SupportedWindowModes.Count() > 1 },
                                                        },
                                                        displayDropdownCore = new SettingsItemV2(displayDropdown = new DisplaySettingsDropdown
                                                        {
                                                            Caption = YTPlayerEXStrings.Display,
                                                            Items = window?.Displays,
                                                            Current = currentDisplay,
                                                        })
                                                        {
                                                            CanBeShown = { BindTarget = displayDropdownCanBeShown }
                                                        },
                                                        resolutionFullscreenDropdownCore = new SettingsItemV2(resolutionFullscreenDropdown = new ResolutionSettingsDropdown
                                                        {
                                                            Caption = YTPlayerEXStrings.ScreenResolution,
                                                            ItemSource = resolutionsFullscreen,
                                                            Current = sizeFullscreen
                                                        })
                                                        {
                                                            ShowRevertToDefaultButton = false,
                                                            CanBeShown = { BindTarget = resolutionFullscreenCanBeShown }
                                                        },
                                                        resolutionWindowedDropdownCore = new SettingsItemV2(resolutionWindowedDropdown = new ResolutionSettingsDropdown
                                                        {
                                                            Caption = YTPlayerEXStrings.ScreenResolution,
                                                            ItemSource = resolutionsWindowed,
                                                            Current = windowedResolution
                                                        })
                                                        {
                                                            ShowRevertToDefaultButton = false,
                                                            CanBeShown = { BindTarget = resolutionWindowedCanBeShown }
                                                        },
                                                        minimiseOnFocusLossCheckboxCore = new SettingsItemV2(new FormCheckBox
                                                        {
                                                            Caption = YTPlayerEXStrings.MinimiseOnFocusLoss,
                                                            Current = config.GetBindable<bool>(FrameworkSetting.MinimiseOnFocusLossInFullscreen),
                                                        }),
                                                        new SettingsItemV2(new RendererSettingsDropdown
                                                        {
                                                            Caption = YTPlayerEXStrings.Renderer,
                                                            Current = renderer,
                                                            Items = host.GetPreferredRenderersForCurrentPlatform().Order()
                                                            #pragma warning disable CS0612 // Type or member is obsolete
                                                            .Where(t => t != RendererType.Vulkan && t != RendererType.OpenGLLegacy),
                                                            #pragma warning restore CS0612 // Type or member is obsolete
                                                        }),
                                                        new SettingsItemV2(new FormCheckBox
                                                        {
                                                            Caption = YTPlayerEXStrings.ShowFPS,
                                                            Current = fpsDisplay,
                                                            Hotkey = new Hotkey(GlobalAction.ToggleFPSDisplay),
                                                        }),
                                                        safeAreaConsiderationsCanBeShown = new SettingsItemV2(new FormCheckBox
                                                        {
                                                            Caption = YTPlayerEXStrings.ShrinkGameToSafeArea,
                                                            Current = appConfig.GetBindable<bool>(YTPlayerEXSetting.SafeAreaConsiderations),
                                                        }),
                                                        new SettingsItemV2(new FormEnumDropdown<ScalingMode>
                                                        {
                                                            Caption = YTPlayerEXStrings.ScreenScaling,
                                                            Current = appConfig.GetBindable<ScalingMode>(YTPlayerEXSetting.Scaling),
                                                            Hotkey = new Hotkey(GlobalAction.CycleScalingMode),
                                                        })
                                                        {
                                                            Keywords = new[] { "scale", "letterbox" },
                                                        },
                                                        scalingSettings = new FillFlowContainer<SettingsItemV2>
                                                        {
                                                            Direction = FillDirection.Vertical,
                                                            RelativeSizeAxes = Axes.X,
                                                            AutoSizeAxes = Axes.Y,
                                                            Masking = true,
                                                            Spacing = new Vector2(0, 4),
                                                            Children = new[]
                                                            {
                                                                new SettingsItemV2(new FormSliderBar<float>
                                                                {
                                                                    Caption = YTPlayerEXStrings.HorizontalPosition,
                                                                    Current = scalingPositionX,
                                                                    KeyboardStep = 0.01f,
                                                                    DisplayAsPercentage = true,
                                                                })
                                                                {
                                                                    Keywords = new[] { "screen", "scaling" },
                                                                },
                                                                new SettingsItemV2(new FormSliderBar<float>
                                                                {
                                                                    Caption = YTPlayerEXStrings.VerticalPosition,
                                                                    Current = scalingPositionY,
                                                                    KeyboardStep = 0.01f,
                                                                    DisplayAsPercentage = true,
                                                                })
                                                                {
                                                                    Keywords = new[] { "screen", "scaling" },
                                                                },
                                                                new SettingsItemV2(new FormSliderBar<float>
                                                                {
                                                                    Caption = YTPlayerEXStrings.HorizontalScale,
                                                                    Current = scalingSizeX,
                                                                    KeyboardStep = 0.01f,
                                                                    DisplayAsPercentage = true,
                                                                })
                                                                {
                                                                    Keywords = new[] { "screen", "scaling" },
                                                                },
                                                                new SettingsItemV2(new FormSliderBar<float>
                                                                {
                                                                    Caption = YTPlayerEXStrings.VerticalScale,
                                                                    Current = scalingSizeY,
                                                                    KeyboardStep = 0.01f,
                                                                    DisplayAsPercentage = true,
                                                                })
                                                                {
                                                                    Keywords = new[] { "screen", "scaling" },
                                                                },
                                                                new SettingsItemV2(dimSlider = new FormSliderBar<float>
                                                                {
                                                                    Caption = YTPlayerEXStrings.ThumbnailDim,
                                                                    Current = scalingBackgroundDim,
                                                                    KeyboardStep = 0.01f,
                                                                    DisplayAsPercentage = true,
                                                                })
                                                            }
                                                        },
                                                        new AdaptiveSpriteText
                                                        {
                                                            Font = YouTubePlayerEXApp.DefaultFont.With(family: "Torus-Alternate", size: 30),
                                                            Text = YTPlayerEXStrings.Screenshot,
                                                            Padding = new MarginPadding { Horizontal = 30, Vertical = 12 },
                                                            Colour = overlayColourProvider.Content2,
                                                        },
                                                        new SettingsItemV2(new FormEnumDropdown<Config.ScreenshotFormat>
                                                        {
                                                            Caption = YTPlayerEXStrings.ScreenshotFormat,
                                                            Current = appConfig.GetBindable<ScreenshotFormat>(YTPlayerEXSetting.ScreenshotFormat)
                                                        }),
                                                        new SettingsItemV2(new FormCheckBox
                                                        {
                                                            Caption = YTPlayerEXStrings.ShowCursorInScreenshots,
                                                            Current = appConfig.GetBindable<bool>(YTPlayerEXSetting.ScreenshotCaptureMenuCursor)
                                                        }),
                                                        new AdaptiveSpriteText
                                                        {
                                                            Font = YouTubePlayerEXApp.DefaultFont.With(family: "Torus-Alternate", size: 30),
                                                            Text = YTPlayerEXStrings.Video,
                                                            Padding = new MarginPadding { Horizontal = 30, Vertical = 12 },
                                                            Colour = overlayColourProvider.Content2,
                                                        },
                                                        new SettingsItemV2(hwAccelCheckbox = new FormCheckBox
                                                        {
                                                            Caption = YTPlayerEXStrings.UseHardwareAcceleration,
                                                        }),
                                                        new SettingsItemV2(new FormEnumDropdown<Config.VideoQuality>
                                                        {
                                                            Caption = YTPlayerEXStrings.VideoQuality,
                                                            Current = videoQuality,
                                                        }),
                                                        new SettingsItemV2(new FormCheckBox
                                                        {
                                                            Caption = YTPlayerEXStrings.AlwaysUseOriginalAudio,
                                                            Current = alwaysUseOriginalAudio,
                                                        }),
                                                        audioLanguageItem = new SettingsItemV2(new FormEnumDropdown<Localisation.Language>
                                                        {
                                                            Caption = YTPlayerEXStrings.AudioLanguage,
                                                            Current = audioLanguage,
                                                        })
                                                        {
                                                            ShowRevertToDefaultButton = false,
                                                        },
                                                        new AdaptiveSpriteText
                                                        {
                                                            Font = YouTubePlayerEXApp.DefaultFont.With(family: "Torus-Alternate", size: 30),
                                                            Text = YTPlayerEXStrings.Audio,
                                                            Padding = new MarginPadding { Horizontal = 30, Vertical = 12 },
                                                            Colour = overlayColourProvider.Content2,
                                                        },
                                                        new SettingsItemV2(new FormCheckBox
                                                        {
                                                            Caption = YTPlayerEXStrings.AdjustPitchOnSpeedChange,
                                                            Current = adjustPitch,
                                                            Hotkey = new Hotkey(GlobalAction.ToggleAdjustPitchOnSpeedChange),
                                                        }),
                                                        new SettingsItemV2(new FormSliderBar<double>
                                                        {
                                                            Caption = YTPlayerEXStrings.MasterVolume,
                                                            Current = config.GetBindable<double>(FrameworkSetting.VolumeUniversal),
                                                            DisplayAsPercentage = true,
                                                        }),
                                                        new SettingsItemV2(new FormSliderBar<double>
                                                        {
                                                            Caption = YTPlayerEXStrings.VideoVolume,
                                                            Current = config.GetBindable<double>(FrameworkSetting.VolumeMusic),
                                                            DisplayAsPercentage = true,
                                                        }),
                                                        new SettingsItemV2(new FormSliderBar<double>
                                                        {
                                                            Caption = YTPlayerEXStrings.SFXVolume,
                                                            Current = config.GetBindable<double>(FrameworkSetting.VolumeEffect),
                                                            DisplayAsPercentage = true,
                                                        }),
                                                        new AdaptiveSpriteText
                                                        {
                                                            Font = YouTubePlayerEXApp.DefaultFont.With(family: "Torus-Alternate", size: 30),
                                                            Text = YTPlayerEXStrings.MochaChanLabs,
                                                            Padding = new MarginPadding { Horizontal = 30, Top = 12 },
                                                            Colour = overlayColourProvider.Content2,
                                                        },
                                                        new AdaptiveTextFlowContainer(f => f.Font = YouTubePlayerEXApp.DefaultFont.With(size: 17, weight: "Regular"))
                                                        {
                                                            RelativeSizeAxes = Axes.X,
                                                            AutoSizeAxes = Axes.Y,
                                                            Text = YTPlayerEXStrings.MochaChanLabsDesc,
                                                            Colour = overlayColourProvider.Background1,
                                                            Padding = new MarginPadding { Horizontal = 30, Bottom = 12 },
                                                        },
                                                        new SettingsItemV2(crt_shader = new FormCheckBox
                                                        {
                                                            Caption = YTPlayerEXStrings.CRTEffect,
                                                        }),
                                                        new SettingsItemV2(pixel_shader = new FormCheckBox
                                                        {
                                                            Caption = YTPlayerEXStrings.PixelEffect,
                                                        }),
                                                        pixel_shader_size_adjust = new SettingsItemV2(new FormSliderBar<float>
                                                        {
                                                            Caption = YTPlayerEXStrings.PixelSize,
                                                            Current = pixelShaderSize,
                                                            LabelFormat = v => $"{v:N2}px",
                                                        }),
                                                        new Container
                                                        {
                                                            RelativeSizeAxes = Axes.X,
                                                            AutoSizeAxes = Axes.Y,
                                                            Margin = new MarginPadding { Top = 12 },
                                                            Child = new Container
                                                            {
                                                                AutoSizeAxes = Axes.Both,
                                                                Anchor = Anchor.Centre,
                                                                Origin = Anchor.Centre,
                                                                Child = new Sprite
                                                                {
                                                                    Width = 100,
                                                                    Height = 100,
                                                                    Texture = textures.Get(@"YouTubePlayerEXLogo"),
                                                                    FillMode = FillMode.Fit,
                                                                }
                                                            },
                                                        },
                                                        new AdaptiveTextFlowContainer(f => f.Font = YouTubePlayerEXApp.DefaultFont.With(family: "Torus-Alternate", size: 30, weight: "Bold"))
                                                        {
                                                            RelativeSizeAxes = Axes.X,
                                                            AutoSizeAxes = Axes.Y,
                                                            Text = "YouTube Player EX",
                                                            TextAnchor = Anchor.Centre,
                                                            Colour = overlayColourProvider.Content2,
                                                        },
                                                        gameVersion = new LinkFlowContainer(f => f.Font = YouTubePlayerEXApp.DefaultFont.With(size: 15))
                                                        {
                                                            RelativeSizeAxes = Axes.X,
                                                            AutoSizeAxes = Axes.Y,
                                                            TextAnchor = Anchor.Centre,
                                                            Colour = overlayColourProvider.Content2,
                                                        },
                                                        madeByText = new LinkFlowContainer(f => f.Font = YouTubePlayerEXApp.DefaultFont.With(size: 15))
                                                        {
                                                            RelativeSizeAxes = Axes.X,
                                                            AutoSizeAxes = Axes.Y,
                                                            TextAnchor = Anchor.Centre,
                                                            Colour = overlayColourProvider.Content2,
                                                        },
                                                        dislikeCounterCredits = new LinkFlowContainer(f => f.Font = YouTubePlayerEXApp.DefaultFont.With(size: 15))
                                                        {
                                                            RelativeSizeAxes = Axes.X,
                                                            AutoSizeAxes = Axes.Y,
                                                            Padding = new MarginPadding { Horizontal = 30, Vertical = 12 },
                                                            TextAnchor = Anchor.Centre,
                                                            Colour = overlayColourProvider.Content2,
                                                        },
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        videoDescriptionContainer = new OverlayContainer
                        {
                            Size = new Vector2(0.7f),
                            RelativeSizeAxes = Axes.Both,
                            CornerRadius = YouTubePlayerEXApp.UI_CORNER_RADIUS,
                            Masking = true,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            EdgeEffect = new osu.Framework.Graphics.Effects.EdgeEffectParameters
                            {
                                Type = osu.Framework.Graphics.Effects.EdgeEffectType.Shadow,
                                Colour = Color4.Black.Opacity(0.25f),
                                Offset = new Vector2(0, 2),
                                Radius = 16,
                            },
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = overlayColourProvider.Background5,
                                },
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding(6),
                                    Spacing = new Vector2(0, 5),
                                    Direction = FillDirection.Vertical,
                                    Children = new Drawable[]
                                    {
                                        videoMetadataDisplayDetails = new VideoMetadataDisplay
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Height = 60,
                                            Origin = Anchor.TopLeft,
                                            Anchor = Anchor.TopLeft,
                                            AlwaysPresent = true,
                                        },
                                        new FillFlowContainer
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Direction = FillDirection.Horizontal,
                                            Spacing = new Vector2(4, 0),
                                            Children = new Drawable[]
                                            {
                                                likeButton = new RoundedButtonContainer
                                                {
                                                    AutoSizeAxes = Axes.X,
                                                    Height = 32,
                                                    CornerRadius = YouTubePlayerEXApp.UI_CORNER_RADIUS,
                                                    Masking = true,
                                                    AlwaysPresent = true,
                                                    Children = new Drawable[]
                                                    {
                                                        new Container
                                                        {
                                                            RelativeSizeAxes = Axes.Both,
                                                            CornerRadius = YouTubePlayerEXApp.UI_CORNER_RADIUS,
                                                            Children = new Drawable[] {
                                                                likeButtonBackground = new Box
                                                                {
                                                                    RelativeSizeAxes = Axes.Both,
                                                                    Colour = overlayColourProvider.Background4,
                                                                    Alpha = 0.7f,
                                                                },
                                                                likeButtonBackgroundSelected = new Box
                                                                {
                                                                    RelativeSizeAxes = Axes.Both,
                                                                    Colour = overlayColourProvider.Content2,
                                                                    Alpha = 0f,
                                                                },
                                                            },
                                                        },
                                                        likeButtonForeground = new FillFlowContainer
                                                        {
                                                            AutoSizeAxes = Axes.X,
                                                            RelativeSizeAxes = Axes.Y,
                                                            Direction = FillDirection.Horizontal,
                                                            Spacing = new Vector2(4, 0),
                                                            Padding = new MarginPadding(8),
                                                            Colour = overlayColourProvider.Content2,
                                                            Children = new Drawable[]
                                                            {
                                                                new SpriteIcon
                                                                {
                                                                    Width = 15,
                                                                    Height = 15,
                                                                    Icon = FontAwesome.Solid.ThumbsUp,
                                                                },
                                                                likeCount = new AdaptiveSpriteText
                                                                {
                                                                    Text = "[no metadata]",
                                                                },
                                                            }
                                                        }
                                                    }
                                                },
                                                dislikeButton = new RoundedButtonContainer
                                                {
                                                    AutoSizeAxes = Axes.X,
                                                    Height = 32,
                                                    CornerRadius = YouTubePlayerEXApp.UI_CORNER_RADIUS,
                                                    Masking = true,
                                                    AlwaysPresent = true,
                                                    Children = new Drawable[]
                                                    {
                                                        new Container
                                                        {
                                                            RelativeSizeAxes = Axes.Both,
                                                            CornerRadius = YouTubePlayerEXApp.UI_CORNER_RADIUS,
                                                            Children = new Drawable[] {
                                                                dislikeButtonBackground = new Box
                                                                {
                                                                    RelativeSizeAxes = Axes.Both,
                                                                    Colour = overlayColourProvider.Background4,
                                                                    Alpha = 0.7f,
                                                                },
                                                                dislikeButtonBackgroundSelected = new Box
                                                                {
                                                                    RelativeSizeAxes = Axes.Both,
                                                                    Colour = overlayColourProvider.Content2,
                                                                    Alpha = 0f,
                                                                },
                                                            },
                                                        },
                                                        dislikeButtonForeground = new FillFlowContainer
                                                        {
                                                            AutoSizeAxes = Axes.X,
                                                            RelativeSizeAxes = Axes.Y,
                                                            Direction = FillDirection.Horizontal,
                                                            Spacing = new Vector2(4, 0),
                                                            Padding = new MarginPadding(8),
                                                            Colour = overlayColourProvider.Content2,
                                                            Children = new Drawable[]
                                                            {
                                                                new SpriteIcon
                                                                {
                                                                    Width = 15,
                                                                    Height = 15,
                                                                    Icon = FontAwesome.Solid.ThumbsDown,
                                                                },
                                                                dislikeCount = new AdaptiveSpriteText
                                                                {
                                                                    Text = "[no metadata]",
                                                                },
                                                            }
                                                        }
                                                    }
                                                },
                                                commentOpenButtonDetails = new RoundedButtonContainer
                                                {
                                                    AutoSizeAxes = Axes.X,
                                                    Height = 32,
                                                    CornerRadius = YouTubePlayerEXApp.UI_CORNER_RADIUS,
                                                    Masking = true,
                                                    AlwaysPresent = true,
                                                    ClickAction = f =>
                                                    {
                                                        if (!commentsDisabled) {
                                                            hideOverlays();
                                                            showOverlayContainer(commentsContainer);
                                                        }
                                                    },
                                                    Children = new Drawable[]
                                                    {
                                                        new Container
                                                        {
                                                            RelativeSizeAxes = Axes.Both,
                                                            CornerRadius = YouTubePlayerEXApp.UI_CORNER_RADIUS,
                                                            Child = new Box
                                                            {
                                                                RelativeSizeAxes = Axes.Both,
                                                                Colour = overlayColourProvider.Background4,
                                                                Alpha = 0.7f,
                                                            },
                                                        },
                                                        new FillFlowContainer
                                                        {
                                                            AutoSizeAxes = Axes.X,
                                                            RelativeSizeAxes = Axes.Y,
                                                            Direction = FillDirection.Horizontal,
                                                            Spacing = new Vector2(4, 0),
                                                            Padding = new MarginPadding(8),
                                                            Children = new Drawable[]
                                                            {
                                                                new SpriteIcon
                                                                {
                                                                    Width = 15,
                                                                    Height = 15,
                                                                    Icon = FontAwesome.Regular.CommentAlt,
                                                                    Colour = overlayColourProvider.Content2,
                                                                },
                                                                commentCount = new AdaptiveSpriteText
                                                                {
                                                                    Text = "[no metadata]",
                                                                    Colour = overlayColourProvider.Content2,
                                                                },
                                                            }
                                                        }
                                                    }
                                                },
                                            }
                                        },
                                        new Container
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Child = new AdaptiveScrollContainer
                                            {
                                                Padding = new MarginPadding()
                                                {
                                                    Bottom = 102,
                                                },
                                                RelativeSizeAxes = Axes.Both,
                                                CornerRadius = YouTubePlayerEXApp.UI_CORNER_RADIUS,
                                                Masking = true,
                                                ScrollbarVisible = false,
                                                Children = new Drawable[]
                                                {
                                                    new Container
                                                    {
                                                        RelativeSizeAxes = Axes.Both,
                                                        CornerRadius = YouTubePlayerEXApp.UI_CORNER_RADIUS,
                                                        Masking = true,
                                                        Child = new Box
                                                        {
                                                            RelativeSizeAxes = Axes.Both,
                                                            Colour = overlayColourProvider.Background4,
                                                            Alpha = 0.7f,
                                                        },
                                                    },
                                                    new FillFlowContainer
                                                    {
                                                        RelativeSizeAxes = Axes.X,
                                                        AutoSizeAxes = Axes.Y,
                                                        CornerRadius = YouTubePlayerEXApp.UI_CORNER_RADIUS,
                                                        Spacing = new Vector2(0, 8),
                                                        Padding = new MarginPadding(12),
                                                        Masking = true,
                                                        Children = new Drawable[]
                                                        {
                                                            videoInfoDetails = new AdaptiveSpriteText
                                                            {
                                                                RelativeSizeAxes = Axes.X,
                                                                Font = YouTubePlayerEXApp.DefaultFont.With(weight: "Black"),
                                                                Colour = overlayColourProvider.Content2,
                                                                AlwaysPresent = true,
                                                            },
                                                            videoDescription = new LinkFlowContainer(f => f.Font = YouTubePlayerEXApp.DefaultFont)
                                                            {
                                                                RelativeSizeAxes = Axes.X,
                                                                AutoSizeAxes = Axes.Y,
                                                                AlwaysPresent = true,
                                                                Colour = overlayColourProvider.Content2,
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                },
                            }
                        },
                        commentsContainer = new OverlayContainer
                        {
                            Size = new Vector2(0.7f),
                            RelativeSizeAxes = Axes.Both,
                            CornerRadius = YouTubePlayerEXApp.UI_CORNER_RADIUS,
                            Masking = true,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            EdgeEffect = new osu.Framework.Graphics.Effects.EdgeEffectParameters
                            {
                                Type = osu.Framework.Graphics.Effects.EdgeEffectType.Shadow,
                                Colour = Color4.Black.Opacity(0.25f),
                                Offset = new Vector2(0, 2),
                                Radius = 16,
                            },
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = overlayColourProvider.Background5,
                                },
                                commentsContainerTitle = new AdaptiveSpriteText
                                {
                                    Origin = Anchor.TopLeft,
                                    Anchor = Anchor.TopLeft,
                                    Text = YTPlayerEXStrings.Comments("0"),
                                    Margin = new MarginPadding(16),
                                    Font = YouTubePlayerEXApp.DefaultFont.With(family: "Torus-Alternate", size: 30, weight: "Bold"),
                                    Colour = overlayColourProvider.Content2,
                                },
                                new GridContainer
                                {
                                    Margin = new MarginPadding
                                    {
                                        Top = 56,
                                    },
                                    Padding = new MarginPadding
                                    {
                                        Horizontal = 16,
                                    },
                                    RelativeSizeAxes = Axes.X,
                                    Height = 45,
                                    ColumnDimensions = new[]
                                    {
                                        new Dimension(),
                                        new Dimension(GridSizeMode.AutoSize),
                                    },
                                    Content = new []
                                    {
                                        new Drawable[]
                                        {
                                            commentTextBox = new AdaptiveTextBox
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                Size = new Vector2(0.97f, 1f),
                                                Text = "",
                                                FontSize = 20,
                                                Height = 45,
                                            },
                                            commentSendButton = new IconButton
                                            {
                                                Origin = Anchor.Centre,
                                                Anchor = Anchor.Centre,
                                                Icon = FontAwesome.Solid.PaperPlane,
                                                Width = 50,
                                                Height = 45,
                                                AlwaysPresent = true,
                                                Enabled = { Value = true },
                                                BackgroundColour = overlayColourProvider.Background3,
                                            },
                                        },
                                    },
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding
                                    {
                                        Horizontal = 16,
                                        Bottom = 16,
                                        Top = (56 * 2),
                                    },
                                    Children = new Drawable[] {
                                        new AdaptiveScrollContainer
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            ScrollbarVisible = false,
                                            Children = new Drawable[]
                                            {
                                                commentContainer = new FillFlowContainer
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    AutoSizeAxes = Axes.Y,
                                                    Spacing = new Vector2(0, 4),
                                                    AlwaysPresent = true,
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        videoInfoExpertOverlay = new OverlayContainer
                        {
                            Size = new Vector2(0.7f),
                            RelativeSizeAxes = Axes.Both,
                            CornerRadius = YouTubePlayerEXApp.UI_CORNER_RADIUS,
                            Masking = true,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            EdgeEffect = new osu.Framework.Graphics.Effects.EdgeEffectParameters
                            {
                                Type = osu.Framework.Graphics.Effects.EdgeEffectType.Shadow,
                                Colour = Color4.Black.Opacity(0.25f),
                                Offset = new Vector2(0, 2),
                                Radius = 16,
                            },
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = overlayColourProvider.Background5,
                                },
                                new AdaptiveSpriteText
                                {
                                    Origin = Anchor.TopLeft,
                                    Anchor = Anchor.TopLeft,
                                    Text = "Video info (Expert)",
                                    Margin = new MarginPadding(16),
                                    Font = YouTubePlayerEXApp.DefaultFont.With(family: "Torus-Alternate", size: 30, weight: "Bold"),
                                    Colour = overlayColourProvider.Content2,
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding
                                    {
                                        Horizontal = 16,
                                        Bottom = 16,
                                        Top = 56,
                                    },
                                    Children = new Drawable[] {
                                        new AdaptiveScrollContainer
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            ScrollbarVisible = false,
                                            Children = new Drawable[]
                                            {
                                                infoForNerds = new AdaptiveTextFlowContainer
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    AutoSizeAxes = Axes.Y,
                                                    Colour = overlayColourProvider.Content2,
                                                    AlwaysPresent = true,
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        searchContainer = new OverlayContainer
                        {
                            Size = new Vector2(0.7f),
                            RelativeSizeAxes = Axes.Both,
                            CornerRadius = YouTubePlayerEXApp.UI_CORNER_RADIUS,
                            Masking = true,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            EdgeEffect = new osu.Framework.Graphics.Effects.EdgeEffectParameters
                            {
                                Type = osu.Framework.Graphics.Effects.EdgeEffectType.Shadow,
                                Colour = Color4.Black.Opacity(0.25f),
                                Offset = new Vector2(0, 2),
                                Radius = 16,
                            },
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = overlayColourProvider.Background5,
                                },
                                new AdaptiveSpriteText
                                {
                                    Origin = Anchor.TopLeft,
                                    Anchor = Anchor.TopLeft,
                                    Text = YTPlayerEXStrings.Search,
                                    Margin = new MarginPadding(16),
                                    Font = YouTubePlayerEXApp.DefaultFont.With(family: "Torus-Alternate", size: 30, weight: "Bold"),
                                    Colour = overlayColourProvider.Content2,
                                },
                                new GridContainer
                                {
                                    Margin = new MarginPadding
                                    {
                                        Top = 56,
                                    },
                                    Padding = new MarginPadding
                                    {
                                        Horizontal = 16,
                                    },
                                    RelativeSizeAxes = Axes.X,
                                    Height = 45,
                                    ColumnDimensions = new[]
                                    {
                                        new Dimension(),
                                        new Dimension(GridSizeMode.AutoSize),
                                    },
                                    Content = new []
                                    {
                                        new Drawable[]
                                        {
                                            searchTextBox = new AdaptiveTextBox
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                Size = new Vector2(0.97f, 1f),
                                                Text = "",
                                                PlaceholderText = YTPlayerEXStrings.SearchPlaceholder,
                                                FontSize = 20,
                                                Height = 45,
                                            },
                                            searchButton = new IconButton
                                            {
                                                Origin = Anchor.Centre,
                                                Anchor = Anchor.Centre,
                                                Icon = FontAwesome.Solid.Search,
                                                Width = 50,
                                                Height = 45,
                                                AlwaysPresent = true,
                                                Enabled = { Value = true },
                                            },
                                        },
                                    },
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding
                                    {
                                        Horizontal = 16,
                                        Bottom = 16,
                                        Top = (56 * 2),
                                    },
                                    Children = new Drawable[] {
                                        new AdaptiveScrollContainer
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            ScrollbarVisible = false,
                                            Children = new Drawable[]
                                            {
                                                searchResultContainer = new FillFlowContainer
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    AutoSizeAxes = Axes.Y,
                                                    Spacing = new Vector2(0, 4),
                                                    AlwaysPresent = true,
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        reportAbuseOverlay = new OverlayContainer
                        {
                            Size = new Vector2(0.7f, 1f),
                            Height = 276,
                            RelativeSizeAxes = Axes.X,
                            CornerRadius = YouTubePlayerEXApp.UI_CORNER_RADIUS,
                            Masking = true,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            EdgeEffect = new osu.Framework.Graphics.Effects.EdgeEffectParameters
                            {
                                Type = osu.Framework.Graphics.Effects.EdgeEffectType.Shadow,
                                Colour = Color4.Black.Opacity(0.25f),
                                Offset = new Vector2(0, 2),
                                Radius = 16,
                            },
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = overlayColourProvider.Background5,
                                },
                                new AdaptiveSpriteText
                                {
                                    Origin = Anchor.TopLeft,
                                    Anchor = Anchor.TopLeft,
                                    Text = YTPlayerEXStrings.Report,
                                    Margin = new MarginPadding(16),
                                    Font = YouTubePlayerEXApp.DefaultFont.With(family: "Torus-Alternate", size: 30, weight: "Bold"),
                                    Colour = overlayColourProvider.Content2,
                                },
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding
                                    {
                                        Horizontal = 16,
                                        Bottom = 16,
                                        Top = 56,
                                    },
                                    Children = new Drawable[] {
                                        new AdaptiveScrollContainer
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            ScrollbarVisible = false,
                                            Children = new Drawable[]
                                            {
                                                new FillFlowContainer
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    AutoSizeAxes = Axes.Y,
                                                    Direction = FillDirection.Vertical,
                                                    Spacing = new Vector2(4),
                                                    Children = new Drawable[]
                                                    {
                                                        new TruncatingSpriteText
                                                        {
                                                            Text = YTPlayerEXStrings.WhatsGoingOn,
                                                            Font = YouTubePlayerEXApp.DefaultFont.With(family: "Torus-Alternate", size: 27, weight: "Bold"),
                                                            Colour = overlayColourProvider.Content2,
                                                        },
                                                        new AdaptiveTextFlowContainer(f => f.Font = YouTubePlayerEXApp.DefaultFont.With(size: 17, weight: "Regular"))
                                                        {
                                                            RelativeSizeAxes = Axes.X,
                                                            AutoSizeAxes = Axes.Y,
                                                            Text = YTPlayerEXStrings.ReportDesc,
                                                            Colour = overlayColourProvider.Background1,
                                                        },
                                                        reportReason = new ReportDropdown
                                                        {
                                                            RelativeSizeAxes = Axes.X,
                                                            Caption = YTPlayerEXStrings.ReportReason,
                                                        },
                                                        reportSubReason = new ReportDropdown
                                                        {
                                                            RelativeSizeAxes = Axes.X,
                                                            Caption = YTPlayerEXStrings.ReportSubReason,
                                                        },
                                                        reportComment = new FormTextBox
                                                        {
                                                            RelativeSizeAxes = Axes.X,
                                                            Height = 50,
                                                            Caption = YTPlayerEXStrings.Description,
                                                        },
                                                        reportButton = new SettingsButtonV2
                                                        {
                                                            Height = 40,
                                                            Text = YTPlayerEXStrings.Submit,
                                                            BackgroundColour = colours.Yellow,
                                                        },
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = alert = new AdaptiveAlertContainer
                            {
                                Origin = Anchor.TopCentre,
                                Anchor = Anchor.TopCentre,
                                Size = new Vector2(600, 60),
                            },
                        }
                    }
                }
            };

            thumbnailContainer.BlurTo(Vector2.Divide(new Vector2(10, 10), 1));
            loadVideoContainer.Hide();
            overlayFadeContainer.Hide();
            settingsContainer.Hide();
            videoDescriptionContainer.Hide();
            commentsContainer.Hide();
            searchContainer.Hide();
            videoInfoExpertOverlay.Hide();
            reportAbuseOverlay.Hide();

            madeByText.AddText("made by ");
            madeByText.AddLink("BoomboxRapsody", "https://github.com/BoomboxRapsody");

            pixel_shader_size_adjust.Hide();

            signedIn.BindValueChanged(loginBool =>
            {
                if (loginBool.NewValue)
                {
                    IList<VideoAbuseReportReasonItem> wth2 = api.GetVideoAbuseReportReasons();

                    foreach (VideoAbuseReportReasonItem wthhh in wth2)
                    {
                        Schedule(() =>
                        {
                            reportReason.AddDropdownItem(wthhh);
                            reportReason.Current.Value = wth2[0];
                        });
                    }

                    Schedule(() => commentSendButton.Enabled.Value = true);
                    Channel wth = api.GetMineChannel();
                    login.Text = YTPlayerEXStrings.SignedIn(api.GetLocalizedChannelTitle(wth, true));

                    if (api.TryToGetMineChannel() != null)
                        commentTextBox.PlaceholderText = YTPlayerEXStrings.CommentWith(api.GetLocalizedChannelTitle(api.GetMineChannel()));
                }
                else
                {
                    Schedule(() => commentSendButton.Enabled.Value = false);
                    login.Text = YTPlayerEXStrings.SignedOut;

                    commentTextBox.PlaceholderText = string.Empty;
                }
            }, true);
            /*
            if (googleOAuth2.SignedIn.Value)
            {
                login.Text = "Signed in";
            }
            else
            {
                login.Text = "Not logged in";
            }
            */

            pixelShaderSize.BindValueChanged(size =>
            {
                pixelShader.Size = new Vector2(size.NewValue);
            }, true);

            crtBackgroundBindable.BindValueChanged(colour =>
            {
                crtShader.BackgroundColour = colour.NewValue;
            }, true);

            pixel_shader.Current.BindValueChanged(enabled =>
            {
                if (enabled.NewValue)
                {
                    appliedEffects.Value.Add(pixelShader);
                    currentVideoSource?.ApplyShaders(appliedEffects.Value);
                    pixel_shader_size_adjust.Show();
                }
                else
                {
                    appliedEffects.Value.Remove(pixelShader);
                    currentVideoSource?.ApplyShaders(appliedEffects.Value);
                    pixel_shader_size_adjust.Hide();
                }
            });

            crt_shader.Current.BindValueChanged(enabled =>
            {
                if (enabled.NewValue)
                {
                    appliedEffects.Value.Add(crtShader);
                    currentVideoSource?.ApplyShaders(appliedEffects.Value);
                }
                else
                {
                    appliedEffects.Value.Remove(crtShader);
                    currentVideoSource?.ApplyShaders(appliedEffects.Value);
                }
            });

            reportReason.Current.BindValueChanged(value =>
            {
                try
                {
                    if (value.NewValue.ContainsSecondaryReasons == true)
                    {
                        reportSubReason.Show();
                        reportSubReason.Items = value.NewValue.SecondaryReasons;
                        reportSubReason.Current.Value = value.NewValue.SecondaryReasons[0];
                    }
                    else
                    {
                        reportSubReason.Hide();
                    }
                }
                catch
                {
                    reportSubReason.Hide();
                }
            }, true);

            commentsDisabled = true;

            playPause.BackgroundColour = searchButton.BackgroundColour = commentSendButton.BackgroundColour = overlayColourProvider.Background3;

            hwAccelCheckbox.Current.Default = hardwareVideoDecoder.Default != HardwareVideoDecoder.None;
            hwAccelCheckbox.Current.Value = hardwareVideoDecoder.Value != HardwareVideoDecoder.None;

            hwAccelCheckbox.Current.BindValueChanged(val =>
            {
                hardwareVideoDecoder.Value = val.NewValue ? HardwareVideoDecoder.Any : HardwareVideoDecoder.None;
            });

            overlayContainers.Add(loadVideoContainer);
            overlayContainers.Add(settingsContainer);
            overlayContainers.Add(videoDescriptionContainer);
            overlayContainers.Add(commentsContainer);
            overlayContainers.Add(videoInfoExpertOverlay);
            overlayContainers.Add(searchContainer);
            overlayContainers.Add(reportAbuseOverlay);

            infoForNerds.AddText("Codec: ");
            infoForNerds.AddText("[unknown]", f => f.Font = YouTubePlayerEXApp.DefaultFont.With(weight: "Bold"));
            infoForNerds.AddText("\nWidth: ");
            infoForNerds.AddText("[unknown]", f => f.Font = YouTubePlayerEXApp.DefaultFont.With(weight: "Bold"));
            infoForNerds.AddText("\nHeight: ");
            infoForNerds.AddText("[unknown]", f => f.Font = YouTubePlayerEXApp.DefaultFont.With(weight: "Bold"));
            infoForNerds.AddText("\nFPS: ");
            infoForNerds.AddText("[unknown]", f => f.Font = YouTubePlayerEXApp.DefaultFont.With(weight: "Bold"));
            infoForNerds.AddText("\nBitrate: ");
            infoForNerds.AddText("[unknown]", f => f.Font = YouTubePlayerEXApp.DefaultFont.With(weight: "Bold"));

            videoQuality.BindValueChanged(quality =>
            {
                videoQualityWarning.Value = (quality.NewValue == Config.VideoQuality.Quality_8K) ? new SettingsNote.Data(YTPlayerEXStrings.VideoQuality8KWarning, SettingsNote.Type.Warning) : null;
                if (currentVideoSource != null)
                {
                    Task.Run(async () =>
                    {
                        await SetVideoSource(videoId, true);
                    });
                }
            });

            alwaysUseOriginalAudio.BindValueChanged(enabled =>
            {
                if (enabled.NewValue)
                {
                    audioLanguageItem.Hide();
                }
                else
                {
                    audioLanguageItem.Show();
                }

                if (currentVideoSource != null)
                {
                    Task.Run(async () =>
                    {
                        await SetVideoSource(videoId, true);
                    });
                }
            }, true);

            adjustPitch.BindValueChanged(value =>
            {
                currentVideoSource?.UpdatePreservePitch(value.NewValue);
            });

            dislikeCounterCredits.AddText(YTPlayerEXStrings.DislikeCounterCredits_1);
            dislikeCounterCredits.AddLink("Return YouTube Dislike API", "https://returnyoutubedislike.com/");
            dislikeCounterCredits.AddText(YTPlayerEXStrings.DislikeCounterCredits_2);

            audioLanguage.BindValueChanged(_ =>
            {
                if (currentVideoSource != null)
                {
                    Task.Run(async () =>
                    {
                        await SetVideoSource(videoId, true);
                    });
                }
            });

            captionLanguage.BindValueChanged(lang =>
            {
                if (currentVideoSource != null)
                {
                    Task.Run(async () =>
                    {
                        var trackManifest = await game.YouTubeClient.Videos.ClosedCaptions.GetManifestAsync(videoUrl);

                        var trackInfo = trackManifest.TryGetByLanguage(api.ParseCaptionLanguage(lang.NewValue));

                        ClosedCaptionTrack captionTrack = null;

                        if (trackInfo != null)
                        {
                            Schedule(() =>
                            {
                                Toast toast = new TrackedSettingToast(new osu.Framework.Configuration.Tracking.SettingDescription(captionLanguage.Value, YTPlayerEXStrings.CaptionLanguage, (trackInfo.IsAutoGenerated ? YTPlayerEXStrings.CaptionAutoGen(captionLanguage.Value.GetLocalisableDescription()) : captionLanguage.Value.GetLocalisableDescription()), "Shift+C"), false);

                                onScreenDisplay.Display(toast);
                            });

                            captionTrack = await game.YouTubeClient.Videos.ClosedCaptions.GetAsync(trackInfo);
                        }

                        currentVideoSource.UpdateCaptionTrack(lang.NewValue, captionTrack);
                    });
                }
            });

            idleTracker.IsIdle.BindValueChanged(idle =>
            {
                if (idle.NewValue == true)
                {
                    hideControls();
                }
                else
                {
                    showControls();
                }
            }, true);

            if (window?.SupportedWindowModes.Count() > 1)
            {
                windowModeDropdownSettings.Show();
            }
            else
            {
                windowModeDropdownSettings.Hide();
            }

            scalingMode.BindValueChanged(_ =>
            {
                scalingSettings.ClearTransforms();
                scalingSettings.AutoSizeDuration = 400;
                scalingSettings.AutoSizeEasing = Easing.OutQuint;

                updateScalingModeVisibility();
            });
            updateScalingModeVisibility();

            videoProgress.BindValueChanged(seek =>
            {
                if (seekbar.IsDragged)
                {
                    currentVideoSource?.SeekTo(seek.NewValue * 1000);
                }
            });

            uiVisible.BindValueChanged(visible =>
            {
                Schedule(() =>
                {
                    if (visible.NewValue)
                    {
                        userInterfaceContainer.Show();
                    }
                    else
                    {
                        userInterfaceContainer.Hide();
                    }
                });
            }, true);

            if (game.IsDeployedBuild)
            {
                gameVersion.AddLink(game.Version, $"https://github.com/BoomboxRapsody/YouTubePlayerEX/releases/{game.Version}", tooltipText: YTPlayerEXStrings.ViewChangelog(game.Version));
            }
            else
            {
                gameVersion.AddText(game.Version);
            }

            updateInfomationText.BindValueChanged(text =>
            {
                checkForUpdatesButton.Text = text.NewValue;
            });

            updateButtonEnabled.BindValueChanged(enabled =>
            {
                checkForUpdatesButton.Enabled.Value = enabled.NewValue;
            });

            renderer.BindValueChanged(r =>
            {
                if (r.NewValue == host.ResolvedRenderer)
                    return;

                // Need to check startup renderer for the "automatic" case, as ResolvedRenderer above will track the final resolved renderer instead.
                if (r.NewValue == RendererType.Automatic && automaticRendererInUse)
                    return;

                if (game?.RestartAppWhenExited() == true)
                {
                    game.Exit();
                }
            });

            void updateScalingModeVisibility()
            {
                try
                {
                    if (scalingMode.Value == ScalingMode.Off)
                        scalingSettings.ResizeHeightTo(0, 400, Easing.OutQuint);

                    scalingSettings.AutoSizeAxes = scalingMode.Value != ScalingMode.Off ? Axes.Y : Axes.None;

                    foreach (SettingsItemV2 item in scalingSettings)
                    {
                        FormSliderBar<float> slider = (FormSliderBar<float>)item.Control;

                        if (slider == dimSlider)
                            item.CanBeShown.Value = scalingMode.Value == ScalingMode.Everything || scalingMode.Value == ScalingMode.Video;
                        else
                        {
                            slider.TransferValueOnCommit = scalingMode.Value == ScalingMode.Everything;
                            item.CanBeShown.Value = scalingMode.Value != ScalingMode.Off;
                        }
                    }
                }
                catch
                {

                }
            }
        }

        [Resolved]
        private ScreenshotManager screenshotManager { get; set; }

        private AdaptiveTextFlowContainer infoForNerds;

        private Bindable<float> scalingPositionX = null!;
        private Bindable<float> scalingPositionY = null!;
        private Bindable<float> scalingSizeX = null!;
        private Bindable<float> scalingSizeY = null!;

        private FormSliderBar<float> dimSlider = null!;
        private FillFlowContainer<SettingsItemV2> scalingSettings = null!;
        private Bindable<ScalingMode> scalingMode = null!;

        private bool automaticRendererInUse;

        private IBindable<bool> uiVisible;

        private void hideControls()
        {
            if (isControlVisible == true)
            {
                isControlVisible = false;
                uiContainer.FadeOutFromOne(250);
                uiGradientContainer.FadeOutFromOne(250);
                sessionStatics.GetBindable<bool>(Static.IsControlVisible).Value = false;
            }
        }

        [Resolved]
        private SessionStatics sessionStatics { get; set; }

        private async Task checkForUpdates()
        {
            if (updateManager == null || game == null)
                return;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            game.UpdateManagerVersionText.Value = YTPlayerEXStrings.CheckingUpdate;

            checkForUpdatesButton.Enabled.Value = false;

            try
            {
                bool foundUpdate = await updateManager.CheckForUpdateAsync(cancellationTokenSource.Token).ConfigureAwait(true);

                if (!foundUpdate)
                {
                    alert.Text = YTPlayerEXStrings.RunningLatestRelease(game.Version);
                    alert.Show();
                    spinnerShow = Scheduler.AddDelayed(alert.Hide, 3000);
                }
            }
            catch
            {
            }
            finally
            {
                game.UpdateManagerVersionText.Value = game.Version;
                checkForUpdatesButton.Enabled.Value = true;
            }
        }

        private SettingsItemV2 resolutionFullscreenDropdownCore, resolutionWindowedDropdownCore, displayDropdownCore, minimiseOnFocusLossCheckboxCore, checkForUpdatesButtonCore;

        private FormCheckBox hwAccelCheckbox;

        private void showControls()
        {
            if (isControlVisible == false)
            {
                isControlVisible = true;
                uiContainer.FadeInFromZero(125);
                uiGradientContainer.FadeInFromZero(125);
                sessionStatics.GetBindable<bool>(Static.IsControlVisible).Value = true;
            }
        }

        private IBindable<bool> cursorInWindow;
#nullable enable
        private IWindow? window;
#nullable disable

        private SettingsItemV2 windowModeDropdownSettings;

        private partial class RoundedSliderBarWithoutTooltip : RoundedSliderBar<double>
        {
            public override LocalisableString TooltipText => "";
        }

        private void updateDisplaySettingsVisibility()
        {
            if (windowModeDropdown.Current.Value == WindowMode.Fullscreen && resolutionsFullscreen.Count > 1)
            {
                resolutionFullscreenDropdownCore.Show();
            }
            else
            {
                resolutionFullscreenDropdownCore.Hide();
            }

            if (windowModeDropdown.Current.Value == WindowMode.Windowed && resolutionsFullscreen.Count > 1)
            {
                resolutionWindowedDropdownCore.Show();
            }
            else
            {
                resolutionWindowedDropdownCore.Hide();
            }

            if (displayDropdown.Items.Count() > 1)
            {
                displayDropdownCore.Show();
            }
            else
            {
                displayDropdownCore.Hide();
            }

            if (RuntimeInfo.IsDesktop && windowModeDropdown.Current.Value == WindowMode.Fullscreen)
            {
                minimiseOnFocusLossCheckboxCore.Show();
            }
            else
            {
                minimiseOnFocusLossCheckboxCore.Hide();
            }

            if (host.Window?.SafeAreaPadding.Value.Total != Vector2.Zero)
            {
                safeAreaConsiderationsCanBeShown.Show();
            }
            else
            {
                safeAreaConsiderationsCanBeShown.Hide();
            }

            /*
        resolutionFullscreenCanBeShown.Value = windowModeDropdown.Current.Value == WindowMode.Fullscreen && resolutionsFullscreen.Count > 1;
        displayDropdownCanBeShown.Value = windowModeDropdown.Current.Value == WindowMode.Windowed && resolutionsWindowed.Count > 1;
        minimiseOnFocusLossCanBeShown.Value = RuntimeInfo.IsDesktop && windowModeDropdown.Current.Value == WindowMode.Fullscreen;
            */
        }

        private readonly BindableList<Size> resolutionsFullscreen = new BindableList<Size>(new[] { new Size(9999, 9999) });
        private readonly BindableList<Size> resolutionsWindowed = new BindableList<Size>();
        private readonly Bindable<Size> windowedResolution = new Bindable<Size>();
        private readonly IBindable<FullscreenCapability> fullscreenCapability = new Bindable<FullscreenCapability>(FullscreenCapability.Capable);

        private Bindable<Size> sizeFullscreen = null!;
        private Bindable<Size> sizeWindowed = null!;

        private readonly BindableBool resolutionFullscreenCanBeShown = new BindableBool(true);
        private readonly BindableBool resolutionWindowedCanBeShown = new BindableBool(true);
        private readonly BindableBool displayDropdownCanBeShown = new BindableBool(true);
        private readonly BindableBool minimiseOnFocusLossCanBeShown = new BindableBool(true);
        private SettingsItemV2 safeAreaConsiderationsCanBeShown;

        private FormDropdown<Size> resolutionFullscreenDropdown = null!;
        private FormDropdown<Size> resolutionWindowedDropdown = null!;
        private FormDropdown<Display> displayDropdown = null!;
        private FormDropdown<WindowMode> windowModeDropdown = null!;

#nullable enable
        private readonly Bindable<SettingsNote.Data?> windowModeDropdownNote = new Bindable<SettingsNote.Data?>();
#nullable disable

        private BindableNumber<double> playbackSpeed = new BindableNumber<double>(1)
        {
            MinValue = 0.1,
            MaxValue = 4,
            Precision = 0.01,
        };

        private partial class DisplaySettingsDropdown : FormDropdown<Display>
        {
            protected override LocalisableString GenerateItemText(Display item)
            {
                return $"{item.Index}: {item.Name} ({item.Bounds.Width}x{item.Bounds.Height})";
            }
        }

        private partial class ResolutionSettingsDropdown : FormDropdown<Size>
        {
            protected override LocalisableString GenerateItemText(Size item)
            {
                if (item == new Size(9999, 9999))
                    return YTPlayerEXStrings.Default;

                return $"{item.Width}x{item.Height}";
            }
        }

#nullable enable
        private IDisposable? duckOperation;
#nullable disable

        private void showOverlayContainer(OverlayContainer overlayContent)
        {
            duckOperation = game.Duck(new DuckParameters
            {
                DuckVolumeTo = 1,
                DuckDuration = 100,
                RestoreDuration = 100,
            });
            isAnyOverlayOpen.Value = true;
            overlayContent.IsVisible = true;
            videoScalingContainer?.BlurTo(new Vector2(4), 250, Easing.OutQuart);
            videoContainer?.BlurTo(new Vector2(4), 250, Easing.OutQuart);
            overlayFadeContainer.FadeTo(0.5f, 250, Easing.OutQuart);
            overlayContent.Show();
            overlayContent.ScaleTo(0.8f);
            overlayContent.ScaleTo(1f, 750, Easing.OutElastic);
            overlayContent.FadeInFromZero(250, Easing.OutQuart);
            overlayShowSample.Play();
        }

        private void hideOverlayContainer(OverlayContainer overlayContent)
        {
            duckOperation?.Dispose();
            overlayContent.IsVisible = false;
            isAnyOverlayOpen.Value = false;
            overlayHideSample.Play();
            videoScalingContainer?.BlurTo(new Vector2(0), 250, Easing.OutQuart);
            videoContainer?.BlurTo(new Vector2(0), 250, Easing.OutQuart);
            overlayFadeContainer.FadeTo(0f, 250, Easing.OutQuart);
            overlayContent.ScaleTo(0.8f, 250, Easing.OutQuart);
            overlayContent.FadeOutFromOne(250, Easing.OutQuart);
        }

        private bool isLoadVideoContainerVisible = false;

        private Bindable<bool> isAnyOverlayOpen;

        private readonly Bindable<Display> currentDisplay = new Bindable<Display>();

        [Resolved]
        private YouTubePlayerEXAppBase app { get; set; }

        [Resolved(canBeNull: true)]
        private UpdateManager updateManager { get; set; }

        private YouTubeVideoPlayer currentVideoSource;

        [Resolved]
        private GameHost host { get; set; } = null!;

        [Resolved]
        private YouTubeAPI api { get; set; }

        public void Search()
        {
            foreach (var item in searchResultContainer.Children)
            {
                item.Expire();
            }

            IList<SearchResult> searchResults = api.GetSearchResult(searchTextBox.Text);
            foreach (SearchResult item in searchResults)
            {
                if (item.Id.Kind == "youtube#video")
                {
                    YouTubeSearchResultView wth = new YouTubeSearchResultView()
                    {
                        RelativeSizeAxes = Axes.X,
                    };

                    searchResultContainer.Add(wth);

                    wth.ClickAction = async _ =>
                    {
                        await SetVideoSource(item.Id.VideoId);
                    };

                    wth.Enabled.Value = true;

                    wth.Data = item;

                    wth.UpdateData();
                }
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (appGlobalConfig.Get<bool>(YTPlayerEXSetting.FinalLoginState) == true)
            {
                Task.Run(async () => await googleOAuth2.SignIn());
            }

            if (!game.IsDeployedBuild)
                checkForUpdatesButtonCore.Hide();

            sessionStatics.GetBindable<bool>(Static.IsControlVisible).Value = true;

            cursorInWindow?.BindValueChanged(active =>
            {
                if (active.NewValue == false)
                {
                    Schedule(() => hideControls());
                }
                else
                {
                    Schedule(() => showControls());
                }
            });

            loadBtn.ClickAction = async _ =>
            {
                await SetVideoSource(videoIdBox.Text);
            };

            searchButton.ClickAction = _ =>
            {
                Search();
            };

            searchOpenButton.ClickAction = _ =>
            {
                if (!commentsDisabled)
                {
                    showOverlayContainer(searchContainer);
                }
            };

            reportOpenButton.ClickAction = _ =>
            {
                showOverlayContainer(reportAbuseOverlay);
            };

            commentOpenButton.ClickAction = _ =>
            {
                if (!commentsDisabled)
                {
                    showOverlayContainer(commentsContainer);
                }
            };

            loadBtnOverlayShow.ClickAction = _ => showOverlayContainer(loadVideoContainer);
            settingsOverlayShowBtn.ClickAction = _ => showOverlayContainer(settingsContainer);

            windowModeDropdown.Current.BindValueChanged(_ =>
            {
                updateDisplaySettingsVisibility();
            }, true);

            currentDisplay.BindValueChanged(display => Schedule(() =>
            {
                if (display.NewValue == null)
                {
                    resolutionsFullscreen.Clear();
                    resolutionsWindowed.Clear();
                    return;
                }

                var buffer = new Bindable<Size>(windowedResolution.Value);
                resolutionWindowedDropdown.Current = buffer;

                var fullscreenResolutions = display.NewValue.DisplayModes
                                                   .Where(m => m.Size.Width >= 800 && m.Size.Height >= 600)
                                                   .OrderByDescending(m => Math.Max(m.Size.Height, m.Size.Width))
                                                   .Select(m => m.Size)
                                                   .Distinct()
                                                   .ToList();
                var windowedResolutions = fullscreenResolutions
                                          .Where(res => res.Width <= display.NewValue.UsableBounds.Width && res.Height <= display.NewValue.UsableBounds.Height)
                                          .ToList();

                resolutionsFullscreen.ReplaceRange(1, resolutionsFullscreen.Count - 1, fullscreenResolutions);
                resolutionsWindowed.ReplaceRange(0, resolutionsWindowed.Count, windowedResolutions);

                resolutionWindowedDropdown.Current = windowedResolution;

                updateDisplaySettingsVisibility();
            }), true);

            windowedResolution.BindValueChanged(size =>
            {
                if (size.NewValue == sizeWindowed.Value || windowModeDropdown.Current.Value != WindowMode.Windowed)
                    return;

                if (window?.WindowState == osu.Framework.Platform.WindowState.Maximised)
                {
                    window.WindowState = osu.Framework.Platform.WindowState.Normal;
                }

                // Adjust only for top decorations (assuming system titlebar).
                // Bottom/left/right borders are ignored as invisible padding, which don't align with the screen.
                var dBounds = currentDisplay.Value.Bounds;
                var dUsable = currentDisplay.Value.UsableBounds;
                float topBar = host.Window?.BorderSize.Value.Top ?? 0;

                int w = Math.Min(size.NewValue.Width, dUsable.Width);
                int h = (int)Math.Min(size.NewValue.Height, dUsable.Height - topBar);

                windowedResolution.Value = new Size(w, h);
                sizeWindowed.Value = windowedResolution.Value;

                float adjustedY = Math.Max(
                    dUsable.Y + (dUsable.Height - h) / 2f,
                    dUsable.Y + topBar // titlebar adjustment
                );
                windowedPositionY.Value = dBounds.Height - h != 0 ? (adjustedY - dBounds.Y) / (dBounds.Height - h) : 0;
                windowedPositionX.Value = dBounds.Width - w != 0 ? (dUsable.X - dBounds.X + (dUsable.Width - w) / 2f) / (dBounds.Width - w) : 0;
            });

            sizeWindowed.BindValueChanged(size =>
            {
                if (size.NewValue != windowedResolution.Value)
                    windowedResolution.Value = size.NewValue;
            });
        }

        private void hideOverlays()
        {
            foreach (var item in overlayContainers)
            {
                if (item.IsVisible == true)
                {
                    hideOverlayContainer(item);
                }
            }
        }

        private List<OverlayContainer> overlayContainers = new List<OverlayContainer>();

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.FastForward_10sec:
                    currentVideoSource?.FastForward10Sec();
                    return true;

                case GlobalAction.FastRewind_10sec:
                    currentVideoSource?.FastRewind10Sec();
                    return true;

                case GlobalAction.DecreasePlaybackSpeed:
                    playbackSpeed.Value -= 0.05;
                    osd.Display(new SpeedChangeToast(playbackSpeed.Value));
                    return true;

                case GlobalAction.IncreasePlaybackSpeed:
                    playbackSpeed.Value += 0.05;
                    osd.Display(new SpeedChangeToast(playbackSpeed.Value));
                    return true;

                case GlobalAction.DecreasePlaybackSpeed2:
                    playbackSpeed.Value -= 0.01;
                    osd.Display(new SpeedChangeToast(playbackSpeed.Value));
                    return true;

                case GlobalAction.IncreasePlaybackSpeed2:
                    playbackSpeed.Value += 0.01;
                    osd.Display(new SpeedChangeToast(playbackSpeed.Value));
                    return true;
            }

            if (e.Repeat)
                return false;

            switch (e.Action)
            {
                case GlobalAction.Back:
                    hideOverlays();
                    return true;

                case GlobalAction.OpenSettings:
                    if (!settingsContainer.IsVisible)
                    {
                        hideOverlays();
                        showOverlayContainer(settingsContainer);
                    }
                    else
                        hideOverlayContainer(settingsContainer);

                    return true;

                case GlobalAction.OpenDescription:
                    if (!videoDescriptionContainer.IsVisible)
                    {
                        hideOverlays();
                        showOverlayContainer(videoDescriptionContainer);
                    }
                    else
                        hideOverlayContainer(videoDescriptionContainer);

                    return true;

                case GlobalAction.ReportAbuse:
                    if (!reportAbuseOverlay.IsVisible)
                    {
                        hideOverlays();
                        showOverlayContainer(reportAbuseOverlay);
                    }
                    else
                        hideOverlayContainer(reportAbuseOverlay);

                    return true;

                case GlobalAction.OpenComments:
                    if (!commentsContainer.IsVisible)
                    {
                        hideOverlays();
                        showOverlayContainer(commentsContainer);
                    }
                    else
                        hideOverlayContainer(commentsContainer);

                    return true;

                case GlobalAction.Select:
                    if (isLoadVideoContainerVisible)
                    {
                        Task.Run(async () =>
                        {
                            try
                            {
                                await SetVideoSource(videoIdBox.Text);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"예외 발생: {ex.Message}");
                            }
                        });
                    }
                    return true;

                case GlobalAction.PlayPause:
                    if (currentVideoSource != null)
                    {
                        if (currentVideoSource.IsPlaying())
                            currentVideoSource.Pause(true);
                        else
                            currentVideoSource.Play(true);
                    }
                    return true;

                case GlobalAction.ToggleAdjustPitchOnSpeedChange:
                    adjustPitch.Value = !adjustPitch.Value;
                    return true;

                case GlobalAction.ToggleFPSDisplay:
                    fpsDisplay.Value = !fpsDisplay.Value;
                    return true;

                case GlobalAction.CycleCaptionLanguage:
                    CycleCaptionLanguage();
                    return true;

                case GlobalAction.CycleAspectRatio:
                    CycleAspectRatio();
                    return true;

                case GlobalAction.CycleScalingMode:
                    CycleScalingMode();
                    return true;
            }

            return false;
        }

        [Resolved]
        private OnScreenDisplay osd { get; set; } = null!;

        protected void CycleCaptionLanguage()
        {
            switch (captionLanguage.Value)
            {
                case ClosedCaptionLanguage.Disabled:
                    captionLanguage.Value = ClosedCaptionLanguage.English;
                    break;

                case ClosedCaptionLanguage.English:
                    captionLanguage.Value = ClosedCaptionLanguage.Korean;
                    break;

                case ClosedCaptionLanguage.Korean:
                    captionLanguage.Value = ClosedCaptionLanguage.Japanese;
                    break;

                case ClosedCaptionLanguage.Japanese:
                    captionLanguage.Value = ClosedCaptionLanguage.Disabled;
                    break;
            }
        }

        protected void CycleScalingMode()
        {
            switch (scalingMode.Value)
            {
                case ScalingMode.Off:
                    scalingMode.Value = ScalingMode.Everything;
                    break;

                case ScalingMode.Everything:
                    scalingMode.Value = ScalingMode.Video;
                    break;

                case ScalingMode.Video:
                    scalingMode.Value = ScalingMode.Off;
                    break;
            }
        }

        protected void CycleAspectRatio()
        {
            switch (aspectRatioMethod.Value)
            {
                case AspectRatioMethod.Letterbox:
                    aspectRatioMethod.Value = AspectRatioMethod.Fill;
                    break;

                case AspectRatioMethod.Fill:
                    aspectRatioMethod.Value = AspectRatioMethod.Letterbox;
                    break;
            }
        }

        private Bindable<bool> videoPlaying;

        protected override void Update()
        {
            base.Update();

            if (currentVideoSource != null)
            {
                playPause.Icon = (currentVideoSource.IsPlaying() ? FontAwesome.Solid.Pause : FontAwesome.Solid.Play);
                playPause.TooltipText = (currentVideoSource.IsPlaying() ? YTPlayerEXStrings.Pause : YTPlayerEXStrings.Play);
                videoProgress.MaxValue = currentVideoSource.VideoProgress.MaxValue;

                videoPlaying.Value = currentVideoSource.IsPlaying();

                TimeSpan duration = TimeSpan.FromSeconds(currentVideoSource.VideoProgress.Value);
                if (duration.Hours > 0)
                {
                    currentTime.Text = $"{duration.Hours.ToString("00")}:{duration.Minutes.ToString("00")}:{duration.Seconds.ToString("00")}";
                }
                else
                {
                    currentTime.Text = $"{duration.Minutes.ToString("0")}:{duration.Seconds.ToString("00")}";
                }

                if (seekbar.IsDragged == false)
                    videoProgress.Value = currentVideoSource.VideoProgress.Value;
            }
        }

        private IconButton playPause;

        private bool commentsDisabled = false;

        /*
        public void GetLocalizedVideoDescriptionRemake(Google.Apis.YouTube.v3.Data.Video videoData)
        {
            string[] splitArg = new string[] { " " };

            string str = api.GetLocalizedVideoDescription(videoData);
            string pattern = @"https?://[^\s/$.?#].[^\s]*"; // Basic URL pattern

            videoDescription.Text = "";

            MatchCollection matches = Regex.Matches(str, pattern);
            foreach (Match match in matches)
            {
                Logger.Log($"Found URL: {match.Value}");
                // To get the end part of a specific match:
                string url = match.Value;
                string lastSegment = url.Split('/').Last();
                if (url.Contains("https://"))
                {
                    videoDescription.AddText(str[..str.IndexOf(url)]);
                    videoDescription.AddLink(str[str.IndexOf("https://")..(url.Length + str.IndexOf("https://"))], str[str.IndexOf("https://")..(url.Length + str.IndexOf("https://"))]);
                    videoDescription.AddText(str[(url.Length + str.IndexOf("https://"))..]);
                }
                else
                {
                    videoDescription.AddText(str[..str.IndexOf("http://")]);
                    videoDescription.AddLink(str[str.IndexOf("http://")..(url.Length + str.IndexOf("https://"))], str[str.IndexOf("http://")..(url.Length + str.IndexOf("https://"))]);
                    videoDescription.AddText(str[(url.Length + str.IndexOf("https://"))..]);
                }
            }
        }
        */

        private partial class PlaybackSpeedSliderBar : RoundedSliderBar<double>
        {
            public override LocalisableString TooltipText => YTPlayerEXStrings.PlaybackSpeed(Current.Value);
        }

        private void updateRatingButtons(string videoId, bool ratingButtonsEnabled)
        {
            if (!googleOAuth2.SignedIn.Value)
                return;

            Task.Run(async () =>
            {
                VideosResource.RateRequest.RatingEnum things = await api.GetVideoRating(videoId);

                switch (things)
                {
                    case VideosResource.RateRequest.RatingEnum.None:
                    {
                        Schedule(() =>
                        {
                            dislikeButtonBackgroundSelected.Hide();
                            likeButtonBackgroundSelected.Hide();
                            likeButtonForeground.Colour = dislikeButtonForeground.Colour = overlayColourProvider1.Content2;

                            if (ratingButtonsEnabled)
                            {
                                likeButton.ClickAction = async _ =>
                                {
                                    await api.RateVideo(videoId, VideosResource.RateRequest.RatingEnum.Like);
                                    Schedule(() =>
                                    {
                                        dislikeButtonBackgroundSelected.Hide();
                                        likeButtonBackgroundSelected.Show();
                                        likeButtonForeground.Colour = overlayColourProvider1.Background4;
                                        dislikeButtonForeground.Colour = overlayColourProvider1.Content2;
                                    });
                                };

                                dislikeButton.ClickAction = async _ =>
                                {
                                    await api.RateVideo(videoId, VideosResource.RateRequest.RatingEnum.Dislike);
                                    Schedule(() =>
                                    {
                                        dislikeButtonBackgroundSelected.Show();
                                        likeButtonBackgroundSelected.Hide();
                                        likeButtonForeground.Colour = overlayColourProvider1.Content2;
                                        dislikeButtonForeground.Colour = overlayColourProvider1.Background4;
                                    });
                                };
                            }
                            else
                            {
                                likeButton.ClickAction = async _ =>
                                {

                                };

                                dislikeButton.ClickAction = async _ =>
                                {

                                };
                            }
                        });
                        break;
                    }
                    case VideosResource.RateRequest.RatingEnum.Like:
                    {
                        Schedule(() =>
                        {
                            dislikeButtonBackgroundSelected.Hide();
                            likeButtonBackgroundSelected.Show();
                            likeButtonForeground.Colour = overlayColourProvider1.Background4;
                            dislikeButtonForeground.Colour = overlayColourProvider1.Content2;

                            if (ratingButtonsEnabled)
                            {
                                likeButton.ClickAction = async _ =>
                                {
                                    await api.RateVideo(videoId, VideosResource.RateRequest.RatingEnum.None);
                                    Schedule(() =>
                                    {
                                        dislikeButtonBackgroundSelected.Hide();
                                        likeButtonBackgroundSelected.Hide();
                                        likeButtonForeground.Colour = dislikeButtonForeground.Colour = overlayColourProvider1.Content2;
                                    });
                                };

                                dislikeButton.ClickAction = async _ =>
                                {
                                    await api.RateVideo(videoId, VideosResource.RateRequest.RatingEnum.Dislike);
                                    Schedule(() =>
                                    {
                                        dislikeButtonBackgroundSelected.Show();
                                        likeButtonBackgroundSelected.Hide();
                                        likeButtonForeground.Colour = overlayColourProvider1.Content2;
                                        dislikeButtonForeground.Colour = overlayColourProvider1.Background4;
                                    });
                                };
                            }
                            else
                            {
                                likeButton.ClickAction = async _ =>
                                {

                                };

                                dislikeButton.ClickAction = async _ =>
                                {

                                };
                            }
                        });
                        break;
                    }
                    case VideosResource.RateRequest.RatingEnum.Dislike:
                    {
                        Schedule(() =>
                        {
                            dislikeButtonBackgroundSelected.Show();
                            likeButtonBackgroundSelected.Hide();
                            likeButtonForeground.Colour = overlayColourProvider1.Content2;
                            dislikeButtonForeground.Colour = overlayColourProvider1.Background4;

                            if (ratingButtonsEnabled)
                            {
                                likeButton.ClickAction = async _ =>
                                {
                                    await api.RateVideo(videoId, VideosResource.RateRequest.RatingEnum.Like);
                                    Schedule(() =>
                                    {
                                        dislikeButtonBackgroundSelected.Hide();
                                        likeButtonBackgroundSelected.Show();
                                        likeButtonForeground.Colour = overlayColourProvider1.Background4;
                                        dislikeButtonForeground.Colour = overlayColourProvider1.Content2;
                                    });
                                };

                                dislikeButton.ClickAction = async _ =>
                                {
                                    await api.RateVideo(videoId, VideosResource.RateRequest.RatingEnum.None);
                                    Schedule(() =>
                                    {
                                        dislikeButtonBackgroundSelected.Hide();
                                        likeButtonBackgroundSelected.Hide();
                                        likeButtonForeground.Colour = dislikeButtonForeground.Colour = overlayColourProvider1.Content2;
                                    });
                                };
                            }
                            else
                            {
                                likeButton.ClickAction = async _ =>
                                {

                                };

                                dislikeButton.ClickAction = async _ =>
                                {

                                };
                            }
                        });
                        break;
                    }
                }
            });
        }

        [Resolved]
        private OverlayColourProvider overlayColourProvider1 { get; set; }

        private void updateVideoMetadata(string videoId)
        {
            videoMetadataDisplay.UpdateVideo(videoId);
            videoMetadataDisplayDetails.UpdateVideo(videoId);
            Task.Run(() =>
            {
                // metadata area
                Google.Apis.YouTube.v3.Data.Video videoData = api.GetVideo(videoId);
                updateRatingButtons(videoId, videoData.Statistics.LikeCount != null);

                Schedule(() => commentOpenButton.Enabled.Value = videoData.Statistics.CommentCount != null);

                if (googleOAuth2.SignedIn.Value)
                    Schedule(() => reportOpenButton.Enabled.Value = true);

                commentsDisabled = videoData.Statistics.CommentCount == null;

                if (videoData.Statistics.CommentCount != null)
                    Schedule(() => commentOpenButtonDetails.Show());
                else
                    Schedule(() => commentOpenButtonDetails.Hide());

                game.RequestUpdateWindowTitle($"{videoData.Snippet.ChannelTitle} - {videoData.Snippet.Title}");

                DateTimeOffset? dateTime = videoData.Snippet.PublishedAtDateTimeOffset;
                DateTime now = DateTime.Now;
                if (!string.IsNullOrEmpty(api.GetLocalizedVideoDescription(videoData)))
                {
                    Schedule(() => videoDescription.Text = api.GetLocalizedVideoDescription(videoData));
                }
                else
                {
                    Schedule(() => videoDescription.AddText(YTPlayerEXStrings.NoDescription, text =>
                    {
                        text.Font = YouTubePlayerEXApp.DefaultFont.With(weight: "SemiBold", italics: true);
                        text.Colour = overlayColourProvider1.Background1;
                    }));
                }
                sessionStatics.GetBindable<string>(Static.CurrentThumbnailUrl).Value = videoData.Snippet.Thumbnails.High.Url;
                commentCount.Text = videoData.Statistics.CommentCount != null ? Convert.ToInt32(videoData.Statistics.CommentCount).ToStandardFormattedString(0) : YTPlayerEXStrings.DisabledByUploader;
                try
                {
                    dislikeCount.Text = ReturnYouTubeDislike.GetDislikes(videoId).Dislikes > 0 ? ReturnYouTubeDislike.GetDislikes(videoId).Dislikes.ToStandardFormattedString(0) : ReturnYouTubeDislike.GetDislikes(videoId).RawDislikes.ToStandardFormattedString(0);
                    dislikeButton.TooltipText = YTPlayerEXStrings.DislikeCountTooltip(ReturnYouTubeDislike.GetDislikes(videoId).Dislikes.ToStandardFormattedString(0), ReturnYouTubeDislike.GetDislikes(videoId).RawDislikes.ToStandardFormattedString(0));
                }
                catch
                {
                    dislikeCount.Text = "0";
                }
                likeCount.Text = videoData.Statistics.LikeCount != null ? Convert.ToInt32(videoData.Statistics.LikeCount).ToStandardFormattedString(0) : ReturnYouTubeDislike.GetDislikes(videoId).RawLikes.ToStandardFormattedString(0);
                commentsContainerTitle.Text = YTPlayerEXStrings.Comments(videoData.Statistics.CommentCount != null ? Convert.ToInt32(videoData.Statistics.CommentCount).ToStandardFormattedString(0) : YTPlayerEXStrings.Disabled);
                videoInfoDetails.Text = YTPlayerEXStrings.VideoMetadataDescWithoutChannelName(Convert.ToInt32(videoData.Statistics.ViewCount).ToStandardFormattedString(0), dateTime.Value.DateTime.Humanize(dateToCompareAgainst: now));

                Schedule(() =>
                {
                    reportButton.Action = () =>
                    {
                        if (!googleOAuth2.SignedIn.Value)
                            return;

                        Toast toast = new Toast(YTPlayerEXStrings.Report, YTPlayerEXStrings.ReportSuccess);
                        api.ReportAbuse(videoId, reportReason.Current.Value.Id, (reportReason.Current.Value.ContainsSecondaryReasons ? reportSubReason.Current.Value.Id : null), (!string.IsNullOrEmpty(reportComment.Current.Value) ? reportComment.Current.Value : null));
                        Schedule(() => onScreenDisplay.Display(toast));
                        reportComment.Current.Value = string.Empty;
                        reportReason.Current.Value = reportReason.Items.ToArray()[0];
                        reportSubReason.Current.Value = reportSubReason.Items.ToArray()[0];
                        hideOverlayContainer(reportAbuseOverlay);
                    };

                    commentSendButton.ClickAction = _ =>
                    {
                        if (!googleOAuth2.SignedIn.Value)
                            return;

                        Toast toast = new Toast(YTPlayerEXStrings.General, YTPlayerEXStrings.CommentAdded);
                        api.SendComment(videoId, commentTextBox.Text);

                        Task.Run(async () =>
                        {
                            Channel myChannel = await api.GetMineChannelAsync();

                            Comment dummy = new Comment();

                            CommentSnippet wth = new CommentSnippet
                            {
                                PublishedAtDateTimeOffset = DateTimeOffset.Now,
                                AuthorChannelId = { Value = myChannel.Id },
                                TextDisplay = commentTextBox.Text,
                                TextOriginal = commentTextBox.Text,
                                LikeCount = 0,
                            };

                            dummy.Snippet = wth;

                            Schedule(() =>
                            {
                                commentContainer.Add(new CommentDisplay(dummy)
                                {
                                    RelativeSizeAxes = Axes.X,
                                });
                            });
                        });

                        Schedule(() => onScreenDisplay.Display(toast));

                        commentTextBox.Text = string.Empty;
                    };
                });

                // comments area
                IList<CommentThread> commentThreadData = api.GetCommentThread(videoId);
                foreach (CommentThread item in commentThreadData)
                {
                    if (item.Snippet.IsPublic == true)
                    {
                        Task.Run(async () =>
                        {
                            Comment comment = await api.GetComment(item.Id);

                            Schedule(() =>
                            {
                                commentContainer.Add(new CommentDisplay(comment)
                                {
                                    RelativeSizeAxes = Axes.X,
                                });
                            });
                        });
                    }
                }

                usernameDisplayMode.BindValueChanged(locale =>
                {
                    Schedule(() =>
                    {
                        if (api.TryToGetMineChannel() != null)
                            commentTextBox.PlaceholderText = YTPlayerEXStrings.CommentWith(api.GetLocalizedChannelTitle(api.GetMineChannel()));
                    });
                }, true);

                localeBindable.BindValueChanged(locale =>
                {
                    Task.Run(async () =>
                    {
                        Schedule(() => videoDescription.Text = api.GetLocalizedVideoDescription(videoData));
                        videoInfoDetails.Text = YTPlayerEXStrings.VideoMetadataDescWithoutChannelName(Convert.ToInt32(videoData.Statistics.ViewCount).ToStandardFormattedString(0), dateTime.Value.DateTime.Humanize(dateToCompareAgainst: now));
                    });
                }, true);

                TimeSpan duration = XmlConvert.ToTimeSpan(videoData.ContentDetails.Duration);
                if (duration.Hours > 0)
                {
                    totalTime.Text = $"{duration.Hours.ToString("0")}:{duration.Minutes.ToString("00")}:{duration.Seconds.ToString("00")}";
                }
                else
                {
                    totalTime.Text = $"{duration.Minutes.ToString("0")}:{duration.Seconds.ToString("00")}";
                }
            });
        }

        private Bindable<UsernameDisplayMode> usernameDisplayMode;

        private void addVideoToScreen()
        {
            videoContainer.Add(currentVideoSource);

            videoLoadingProgress.Text = "";

            videoProgress.BindValueChanged(seek =>
            {
                if (currentVideoSource != null && currentVideoSource.IsPlaying() == false)
                    seekTo(seek.NewValue * 1000);
            });

            playbackSpeed.BindValueChanged(speed =>
            {
                setPlaybackSpeed(speed.NewValue);
            }, true);
        }

        private void seekTo(double pos)
        {
            currentVideoSource?.SeekTo(pos);
        }

        private void setPlaybackSpeed(double speed)
        {
            currentVideoSource?.SetPlaybackSpeed(speed);
        }

        private void playVideo()
        {
            currentVideoSource.Play();
        }

        private string videoUrl = string.Empty;
        private string videoId = string.Empty;
        private double pausedTime = 0;

        [Resolved]
        private YTPlayerEXConfigManager appGlobalConfig { get; set; }

        public async Task SetVideoSource(string videoId, bool clearCache = false)
        {
            this.videoId = videoId;
            pausedTime = clearCache ? currentVideoSource.VideoProgress.Value : 0;
            Schedule(() => currentVideoSource?.Expire());
            if (loadVideoContainer.IsVisible == true)
            {
                Schedule(() => hideOverlayContainer(loadVideoContainer));
            }
            if (searchContainer.IsVisible == true)
            {
                Schedule(() => hideOverlayContainer(searchContainer));
            }
            videoIdBox.Text = string.Empty;

            foreach (var item in commentContainer.Children)
            {
                Schedule(() => item.Expire());
            }

            if (clearCache == true)
            {
                await Task.Delay(1000); // Wait for any ongoing operations to complete
                foreach (var cacheItem in Directory.GetFiles(app.Host.CacheStorage.GetStorageForDirectory("videos").GetFullPath($"{videoId}")))
                {
                    File.Delete(cacheItem);
                }
                Directory.Delete(app.Host.CacheStorage.GetStorageForDirectory("videos").GetFullPath($"{videoId}"));
            }

            if (videoId.Length != 0)
            {
                Google.Apis.YouTube.v3.Data.Video videoData = api.GetVideo(videoId);

                if (videoData.Status.PrivacyStatus == "private")
                {
                    Schedule(() =>
                    {
                        Toast toast = new Toast(YTPlayerEXStrings.General, YTPlayerEXStrings.CannotPlayPrivateVideos);

                        onScreenDisplay.Display(toast);
                    });
                    return;
                }

                IProgress<double> audioDownloadProgress = new Progress<double>((percent) => videoLoadingProgress.Text = $"Downloading audio cache: {(percent * 100):N0}%");
                IProgress<double> videoDownloadProgress = new Progress<double>((percent) => videoLoadingProgress.Text = $"Downloading video cache: {(percent * 100):N0}%");

                spinnerShow = Scheduler.AddDelayed(spinner.Show, 0);

                Schedule(() => videoProgress.MaxValue = 1);
                videoUrl = $"https://youtube.com/watch?v={videoId}";

                spinnerShow = Scheduler.AddDelayed(() => updateVideoMetadata(videoId), 0);
                Schedule(() => thumbnailContainer.Show());

                if (!File.Exists(app.Host.CacheStorage.GetStorageForDirectory("videos").GetFullPath($"{videoId}") + @"/audio.mp3") || !File.Exists(app.Host.CacheStorage.GetStorageForDirectory("videos").GetFullPath($"{videoId}") + @"/video.mp4"))
                {
                    Schedule(() => videoQuality.Disabled = audioLanguage.Disabled = alwaysUseOriginalAudio.Disabled = true);

                    Directory.CreateDirectory(app.Host.CacheStorage.GetStorageForDirectory("videos").GetFullPath($"{videoId}"));

                    var streamManifest = await app.YouTubeClient.Videos.Streams.GetManifestAsync(videoUrl);

                    IStreamInfo audioStreamInfo;

                    try
                    {
                        if (alwaysUseOriginalAudio.Value == true)
                        {
                            Logger.Log($"Preferred audio language is: {videoData.Snippet.DefaultLanguage}");
                            // Select best audio stream (highest bitrate)
                            audioStreamInfo = streamManifest
                                .GetAudioOnlyStreams()
                                .Where(s => s.AudioLanguage.Value.Code.Contains(videoData.Snippet.DefaultLanguage))
                                .TryGetWithHighestBitrate();
                        }
                        else
                        {
                            Logger.Log($"Preferred audio language is: {appGlobalConfig.Get<Language>(YTPlayerEXSetting.AudioLanguage).ToString()}");
                            // Select best audio stream (highest bitrate)
                            audioStreamInfo = streamManifest
                                .GetAudioOnlyStreams()
                                .Where(s => s.AudioLanguage.Value.Code.Contains(appGlobalConfig.Get<Language>(YTPlayerEXSetting.AudioLanguage).ToString()))
                                .TryGetWithHighestBitrate();
                        }
                    }
                    catch (Exception e)
                    {
                        try
                        {
                            // Select best audio stream (highest bitrate)
                            audioStreamInfo = streamManifest
                                .GetAudioOnlyStreams()
                                .Where(s => s.AudioLanguage.Value.Code.Contains(videoData.Snippet.DefaultLanguage))
                                .TryGetWithHighestBitrate();
                            Logger.Error(e, e.GetDescription());
                            Logger.Log($"Prefer default audio language: {videoData.Snippet.DefaultLanguage}");
                        } catch
                        {
                            Logger.Log($"Prefer default audio language failed.\nFalling back to default audio language.");
                            // Select best audio stream (highest bitrate)
                            audioStreamInfo = streamManifest
                                .GetAudioOnlyStreams()
                                .TryGetWithHighestBitrate();
                        }
                    }

                    IVideoStreamInfo videoStreamInfo;

                    if (videoQuality.Value == Config.VideoQuality.PreferHighQuality)
                    {
                        // Select best video stream (1080p60 in this example)
                        videoStreamInfo = streamManifest
                            .GetVideoOnlyStreams()
                            .Where(s => s.Container == YoutubeExplode.Videos.Streams.Container.Mp4)
                            .TryGetWithHighestVideoQuality();
                    }
                    else
                    {
                        // Select best video stream (1080p60 in this example)
                        videoStreamInfo = streamManifest
                            .GetVideoOnlyStreams()
                            .Where(s => s.Container == YoutubeExplode.Videos.Streams.Container.Mp4)
                            .Where(s => s.VideoQuality.Label.Contains(app.ParseVideoQuality()))
                            .TryGetWithHighestVideoQuality();
                    }

                    var trackManifest = await game.YouTubeClient.Videos.ClosedCaptions.GetManifestAsync(videoUrl);

                    var trackInfo = trackManifest.TryGetByLanguage(api.ParseCaptionLanguage(captionLanguage.Value));

                    ClosedCaptionTrack captionTrack = null;

                    if (trackInfo != null)
                    {
                        if (captionLanguage.Value != ClosedCaptionLanguage.Disabled)
                        {
                            Schedule(() =>
                            {
                                /*
                                alert.Text = captionLanguage.Value != ClosedCaptionLanguage.Disabled ? (trackInfo.IsAutoGenerated ? YTPlayerEXStrings.SelectedCaptionAutoGen(captionLanguage.Value.GetLocalisableDescription()) : YTPlayerEXStrings.SelectedCaption(captionLanguage.Value.GetLocalisableDescription())) : YTPlayerEXStrings.SelectedCaption(captionLanguage.Value.GetLocalisableDescription());
                                alert.Show();
                                spinnerShow = Scheduler.AddDelayed(alert.Hide, 3000);
                                */

                                Toast toast = new TrackedSettingToast(new osu.Framework.Configuration.Tracking.SettingDescription(captionLanguage.Value, YTPlayerEXStrings.CaptionLanguage, (trackInfo.IsAutoGenerated ? YTPlayerEXStrings.CaptionAutoGen(captionLanguage.Value.GetLocalisableDescription()) : captionLanguage.Value.GetLocalisableDescription()), "Shift+C"), false);

                                onScreenDisplay.Display(toast);
                            });
                        }

                        captionTrack = await game.YouTubeClient.Videos.ClosedCaptions.GetAsync(trackInfo);
                    }

                    await app.YouTubeClient.Videos.DownloadAsync([audioStreamInfo], new ConversionRequestBuilder(app.Host.CacheStorage.GetStorageForDirectory("videos").GetFullPath($"{videoId}") + @"\audio.mp3").Build(), audioDownloadProgress);
                    await app.YouTubeClient.Videos.DownloadAsync([videoStreamInfo], new ConversionRequestBuilder(app.Host.CacheStorage.GetStorageForDirectory("videos").GetFullPath($"{videoId}") + @"\video.mp4").Build(), videoDownloadProgress);

                    currentVideoSource = new YouTubeVideoPlayer(app.Host.CacheStorage.GetStorageForDirectory("videos").GetFullPath($"{videoId}") + @"/video.mp4", app.Host.CacheStorage.GetStorageForDirectory("videos").GetFullPath($"{videoId}") + @"/audio.mp3", captionTrack, captionLanguage.Value, pausedTime)
                    {
                        RelativeSizeAxes = Axes.Both
                    };

                    if (appliedEffects.Value.Count > 0)
                        Schedule(() => currentVideoSource?.ApplyShaders(appliedEffects.Value));

                    spinnerShow = Scheduler.AddDelayed(spinner.Hide, 0);

                    spinnerShow = Scheduler.AddDelayed(addVideoToScreen, 0);

                    spinnerShow = Scheduler.AddDelayed(() => playVideo(), 0);
                    Schedule(() => thumbnailContainer.Hide());

                    Schedule(() => videoQuality.Disabled = audioLanguage.Disabled = alwaysUseOriginalAudio.Disabled = false);
                }
                else
                {
                    var trackManifest = await game.YouTubeClient.Videos.ClosedCaptions.GetManifestAsync(videoUrl);

                    var trackInfo = trackManifest.TryGetByLanguage(api.ParseCaptionLanguage(captionLanguage.Value));

                    ClosedCaptionTrack captionTrack = null;

                    if (trackInfo != null)
                    {
                        if (captionLanguage.Value != ClosedCaptionLanguage.Disabled)
                        {
                            Schedule(() =>
                            {
                                Toast toast = new TrackedSettingToast(new osu.Framework.Configuration.Tracking.SettingDescription(captionLanguage.Value, YTPlayerEXStrings.CaptionLanguage, (trackInfo.IsAutoGenerated ? YTPlayerEXStrings.CaptionAutoGen(captionLanguage.Value.GetLocalisableDescription()) : captionLanguage.Value.GetLocalisableDescription()), "Shift+C"), false);

                                onScreenDisplay.Display(toast);
                            });
                        }

                        captionTrack = await game.YouTubeClient.Videos.ClosedCaptions.GetAsync(trackInfo);
                    }

                    currentVideoSource = new YouTubeVideoPlayer(app.Host.CacheStorage.GetStorageForDirectory("videos").GetFullPath($"{videoId}") + @"/video.mp4", app.Host.CacheStorage.GetStorageForDirectory("videos").GetFullPath($"{videoId}") + @"/audio.mp3", captionTrack, captionLanguage.Value, pausedTime)
                    {
                        RelativeSizeAxes = Axes.Both
                    };

                    if (appliedEffects.Value.Count > 0)
                        Schedule(() => currentVideoSource?.ApplyShaders(appliedEffects.Value));

                    spinnerShow = Scheduler.AddDelayed(spinner.Hide, 0);

                    spinnerShow = Scheduler.AddDelayed(addVideoToScreen, 0);

                    spinnerShow = Scheduler.AddDelayed(() => playVideo(), 0);
                    Schedule(() => thumbnailContainer.Hide());
                }
            }
            else
            {
                Toast toast = new Toast(YTPlayerEXStrings.General, YTPlayerEXStrings.NoVideoIdError);

                onScreenDisplay.Display(toast);
            }
        }

        private Storage exportStorage = null!;

        [Resolved]
        private OnScreenDisplay onScreenDisplay { get; set; }

#nullable enable
        private void exportLogs()
        {
            const string archive_filename = "compressed-logs.zip";

            try
            {
                GlobalStatistics.OutputToLog();
                Logger.Flush();

                var logStorage = Logger.Storage;

                using (var outStream = exportStorage.CreateFileSafely(archive_filename))
                using (var zip = ZipArchive.Create())
                {
                    foreach (string? f in logStorage.GetFiles(string.Empty, "*.log"))
                        FileUtils.AttemptOperation(z => z.AddEntry(f, logStorage.GetStream(f), closeStream: true), zip, throwOnFailure: false);

                    zip.SaveTo(outStream);
                }
            }
            catch
            {
                // cleanup if export is failed or canceled.
                exportStorage.Delete(archive_filename);
                throw;
            }

            Schedule(() =>
            {
                Toast toast = new Toast(YTPlayerEXStrings.General, YTPlayerEXStrings.LogsExportFinished);

                onScreenDisplay.Display(toast);
                exportStorage.PresentFileExternally(archive_filename);
            });
        }
#nullable disable

        [Resolved]
        private YouTubePlayerEXApp game { get; set; }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

#nullable enable
        /// <summary>
        /// Contrary to <see cref="Display.Equals(osu.Framework.Platform.Display?)"/>, this comparer disregards the value of <see cref="Display.Bounds"/>.
        /// We want to just show a list of displays, and for the purposes of settings we don't care about their bounds when it comes to the list.
        /// However, <see cref="IWindow.DisplaysChanged"/> fires even if only the resolution of the current display was changed
        /// (because it causes the bounds of all displays to also change).
        /// We're not interested in those changes, so compare only the rest that we actually care about.
        /// This helps to avoid a bindable/event feedback loop, in which a resolution change
        /// would trigger a display "change", which would in turn reset resolution again.
        /// </summary>
        private class DisplayListComparer : IEqualityComparer<Display>
        {
            public static readonly DisplayListComparer DEFAULT = new DisplayListComparer();

            public bool Equals(Display? x, Display? y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;

                return x.Index == y.Index
                       && x.Name == y.Name
                       && x.DisplayModes.SequenceEqual(y.DisplayModes);
            }

            public int GetHashCode(Display obj)
            {
                var hashCode = new HashCode();

                hashCode.Add(obj.Index);
                hashCode.Add(obj.Name);
                hashCode.Add(obj.DisplayModes.Length);
                foreach (var displayMode in obj.DisplayModes)
                    hashCode.Add(displayMode);

                return hashCode.ToHashCode();
            }
        }
#nullable disable

        private partial class RendererSettingsDropdown : FormEnumDropdown<RendererType>
        {
            private RendererType hostResolvedRenderer;
            private bool automaticRendererInUse;

            [BackgroundDependencyLoader]
            private void load(FrameworkConfigManager config, GameHost host)
            {
                var renderer = config.GetBindable<RendererType>(FrameworkSetting.Renderer);
                automaticRendererInUse = renderer.Value == RendererType.Automatic;
                hostResolvedRenderer = host.ResolvedRenderer;
            }

            protected override LocalisableString GenerateItemText(RendererType item)
            {
                if (item == RendererType.Automatic && automaticRendererInUse)
                    return YTPlayerEXStrings.RenderTypeAutomaticIsUse(hostResolvedRenderer.GetDescription());

                if (item == RendererType.Automatic)
                {
                    return YTPlayerEXStrings.RenderTypeAutomatic;
                }

                return base.GenerateItemText(item);
            }
        }

        private partial class ReportDropdown : FormDropdown<VideoAbuseReportReasonItem>
        {
            protected override LocalisableString GenerateItemText(VideoAbuseReportReasonItem item)
            {
                return item.Label;
            }
        }

        private partial class WindowModeDropdown : FormDropdown<WindowMode>
        {
            protected override LocalisableString GenerateItemText(WindowMode item)
            {
                switch (item)
                {
                    case WindowMode.Windowed:
                        return YTPlayerEXStrings.Windowed;

                    case WindowMode.Borderless:
                        return YTPlayerEXStrings.Borderless;

                    case WindowMode.Fullscreen:
                        return YTPlayerEXStrings.Fullscreen;
                }
                return base.GenerateItemText(item);
            }
        }

        private partial class FrameSyncDropdown : FormEnumDropdown<FrameSync>
        {
            protected override LocalisableString GenerateItemText(FrameSync item)
            {
                switch (item)
                {
                    case FrameSync.VSync:
                        return YTPlayerEXStrings.VSync;

                    case FrameSync.Limit2x:
                        return YTPlayerEXStrings.RefreshRate2X;

                    case FrameSync.Limit4x:
                        return YTPlayerEXStrings.RefreshRate4X;

                    case FrameSync.Limit8x:
                        return YTPlayerEXStrings.RefreshRate8X;

                    case FrameSync.Unlimited:
                        return YTPlayerEXStrings.Unlimited;
                }
                return base.GenerateItemText(item);
            }
        }
    }
}
