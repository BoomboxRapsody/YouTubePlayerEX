using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
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
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
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
using YouTubePlayerEX.App.Graphics.Sprites;
using YouTubePlayerEX.App.Graphics.UserInterface;
using YouTubePlayerEX.App.Graphics.UserInterfaceV2;
using YouTubePlayerEX.App.Graphics.Videos;
using YouTubePlayerEX.App.Input;
using YouTubePlayerEX.App.Input.Binding;
using YouTubePlayerEX.App.Localisation;
using YouTubePlayerEX.App.Online;
using YouTubePlayerEX.App.Updater;
using YouTubePlayerEX.App.Utils;
using static SharpGen.Runtime.TypeDataStorage;
using static YouTubePlayerEX.App.YouTubePlayerEXApp;
using Container = osu.Framework.Graphics.Containers.Container;
using Language = YouTubePlayerEX.App.Localisation.Language;
using OverlayContainer = YouTubePlayerEX.App.Graphics.Containers.OverlayContainer;

namespace YouTubePlayerEX.App.Screens
{
    public partial class MainAppView : YouTubePlayerEXScreen, IKeyBindingHandler<GlobalAction>
    {
        private BufferedContainer videoContainer;
        private AdaptiveButton loadBtn;
        private AdaptiveTextBox videoIdBox;
        private LoadingSpinner spinner;
        private ScheduledDelegate spinnerShow;
        private AdaptiveAlertContainer alert;
        private IdleTracker idleTracker;
        private Container uiContainer;
        private Container uiGradientContainer;
        private OverlayContainer loadVideoContainer, settingsContainer, videoDescriptionContainer, commentsContainer;
        private AdaptiveButton loadBtnOverlayShow, settingsOverlayShowBtn, commentOpenButton;
        private VideoMetadataDisplayWithoutProfile videoMetadataDisplay;
        private VideoMetadataDisplay videoMetadataDisplayDetails;
        private RoundedButtonContainer commentOpenButtonDetails;

        private Sample overlayShowSample;
        private Sample overlayHideSample;

        private Container overlayFadeContainer;

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            // Be sure to dispose the track, otherwise memory will be leaked!
            // This is automatic for DrawableTrack.
            overlayShowSample.Dispose();
            overlayHideSample.Dispose();
        }

        private AdaptiveSpriteText videoLoadingProgress, videoInfoDetails, likeCount, dislikeCount, commentCount, commentsContainerTitle, currentTime, totalTime;
        private TextFlowContainer videoDescription;
        private FillFlowContainer commentContainer;

        private BindableNumber<double> videoProgress = new BindableNumber<double>()
        {
            MinValue = 0,
            MaxValue = 1,
        };

        private Bindable<double> windowedPositionX = null!;
        private Bindable<double> windowedPositionY = null!;
        private Bindable<WindowMode> windowMode = null!;

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
        private FormCheckUpdateButton checkForUpdatesButton;
        private ThumbnailContainer thumbnailContainer;
        private AdaptiveSliderBar<double> seekbar;
        private Bindable<LocalisableString> updateInfomationText;
        private Bindable<bool> updateButtonEnabled;

        [Resolved]
        private AdaptiveColour colours { get; set; } = null!;

        private Bindable<SettingsNote.Data> videoQualityWarning = new Bindable<SettingsNote.Data>();

        [BackgroundDependencyLoader]
        private void load(ISampleStore sampleStore, FrameworkConfigManager config, YTPlayerEXConfigManager appConfig, GameHost host, Storage storage)
        {
            window = host.Window;

            var renderer = config.GetBindable<RendererType>(FrameworkSetting.Renderer);
            automaticRendererInUse = renderer.Value == RendererType.Automatic;

            exportStorage = storage.GetStorageForDirectory(@"exports");

            localeBindable = config.GetBindable<string>(FrameworkSetting.Locale);
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

            windowedResolution.Value = sizeWindowed.Value;

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
                new ParallaxContainer
                {
                    Children = new Drawable[]
                    {
                        thumbnailContainer = new ThumbnailContainer
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
                videoContainer = new BufferedContainer
                {
                    RelativeSizeAxes = Axes.Both,
                },
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
                            Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0.8f), Color4.Black.Opacity(0)),
                            Origin = Anchor.TopLeft,
                            Anchor = Anchor.TopLeft,
                            RelativeSizeAxes = Axes.X,
                            Height = 300,
                        },
                        new Box
                        {
                            Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0), Color4.Black.Opacity(0.8f)),
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
                        loadBtnOverlayShow = new IconButton
                        {
                            Enabled = { Value = true },
                            Origin = Anchor.TopRight,
                            Anchor = Anchor.TopRight,
                            Size = new Vector2(40, 40),
                            Icon = FontAwesome.Regular.FolderOpen,
                            IconScale = new Vector2(1.2f),
                            TooltipText = YTPlayerEXStrings.LoadVideo,
                        },
                        settingsOverlayShowBtn = new IconButton
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
                        commentOpenButton = new IconButton
                        {
                            Enabled = { Value = true },
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
                        new Container {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            RelativeSizeAxes = Axes.X,
                            Height = 100,
                            Masking = true,
                            CornerRadius = 12,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = Color4.White,
                                    Alpha = 0.1f,
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
                                                },
                                                totalTime = new AdaptiveSpriteText
                                                {
                                                    Anchor = Anchor.TopRight,
                                                    Origin = Anchor.TopRight,
                                                    Text = "0:00",
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
                                                    CornerRadius = 12,
                                                    Children = new Drawable[]
                                                    {
                                                        new Box
                                                        {
                                                            RelativeSizeAxes = Axes.Both,
                                                            Colour = Color4.White,
                                                            Alpha = 0.1f,
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
                    CornerRadius = 12,
                    Masking = true,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4Extensions.FromHex("#1a1a1a"),
                        },
                        new AdaptiveSpriteText
                        {
                            Origin = Anchor.TopLeft,
                            Anchor = Anchor.TopLeft,
                            Text = YTPlayerEXStrings.LoadFromVideoId,
                            Margin = new MarginPadding(16),
                            Font = YouTubePlayerEXApp.DefaultFont.With(size: 30),
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
                    CornerRadius = 12,
                    Masking = true,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4Extensions.FromHex("#1a1a1a"),
                        },
                        new AdaptiveSpriteText
                        {
                            Origin = Anchor.TopLeft,
                            Anchor = Anchor.TopLeft,
                            Text = YTPlayerEXStrings.Settings,
                            Margin = new MarginPadding(16),
                            Font = YouTubePlayerEXApp.DefaultFont.With(size: 30),
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
                                            Children = new Drawable[] {
                                                new AdaptiveSpriteText
                                                {
                                                    Font = YouTubePlayerEXApp.DefaultFont.With(size: 30),
                                                    Text = YTPlayerEXStrings.General,
                                                    Padding = new MarginPadding { Horizontal = 30, Bottom = 12 }
                                                },
                                                new SettingsButtonV2
                                                {
                                                    Text = YTPlayerEXStrings.ExportLogs,
                                                    Padding = new MarginPadding { Horizontal = 30 },
                                                    BackgroundColour = colours.YellowDarker.Darken(0.5f),
                                                    Action = () => Task.Run(exportLogs),
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
                                                }),
                                                new SettingsItemV2(new FormEnumDropdown<VideoMetadataTranslateSource>
                                                {
                                                    Caption = YTPlayerEXStrings.VideoMetadataTranslateSource,
                                                    Current = appConfig.GetBindable<VideoMetadataTranslateSource>(YTPlayerEXSetting.VideoMetadataTranslateSource),
                                                }),
                                                checkForUpdatesButtonCore = new SettingsItemV2(checkForUpdatesButton = new FormCheckUpdateButton
                                                {
                                                    Caption = YTPlayerEXStrings.CheckUpdate,
                                                    Text = app.Version,
                                                    Action = () => {
                                                        if (game.RestartRequired.Value != true)
                                                            checkForUpdates().FireAndForget();
                                                        else
                                                            game.RestartAction.Invoke();
                                                    },
                                                }),
                                                new AdaptiveSpriteText
                                                {
                                                    Font = YouTubePlayerEXApp.DefaultFont.With(size: 30),
                                                    Text = YTPlayerEXStrings.Graphics,
                                                    Padding = new MarginPadding { Horizontal = 30, Vertical = 12 }
                                                },
                                                new SettingsItemV2(new FormEnumDropdown<AspectRatioMethod>
                                                {
                                                    Caption = YTPlayerEXStrings.AspectRatioMethod,
                                                    Current = appConfig.GetBindable<AspectRatioMethod>(YTPlayerEXSetting.AspectRatioMethod),
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
                                                new SettingsItemV2(new FormEnumDropdown<FrameSync>
                                                {
                                                    Caption = YTPlayerEXStrings.FrameLimiter,
                                                    Current = config.GetBindable<FrameSync>(FrameworkSetting.FrameSync),
                                                }),
                                                windowModeDropdownSettings = new SettingsItemV2(windowModeDropdown = new FormDropdown<WindowMode>
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
                                                    Current = appConfig.GetBindable<bool>(YTPlayerEXSetting.ShowFpsDisplay),
                                                }),
                                                new AdaptiveSpriteText
                                                {
                                                    Font = YouTubePlayerEXApp.DefaultFont.With(size: 30),
                                                    Text = YTPlayerEXStrings.Video,
                                                    Padding = new MarginPadding { Horizontal = 30, Vertical = 12 }
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
                                                new SettingsItemV2(new FormEnumDropdown<Localisation.Language>
                                                {
                                                    Caption = YTPlayerEXStrings.AudioLanguage,
                                                    Current = audioLanguage,
                                                })
                                                {
                                                    ShowRevertToDefaultButton = false,
                                                },
                                                new AdaptiveSpriteText
                                                {
                                                    Font = YouTubePlayerEXApp.DefaultFont.With(size: 30),
                                                    Text = YTPlayerEXStrings.Audio,
                                                    Padding = new MarginPadding { Horizontal = 30, Vertical = 12 }
                                                },
                                                new SettingsItemV2(new FormCheckBox
                                                {
                                                    Caption = YTPlayerEXStrings.AdjustPitchOnSpeedChange,
                                                    Current = adjustPitch,
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
                                                new AdaptiveTextFlowContainer(f => f.Font = YouTubePlayerEXApp.DefaultFont.With(size: 15))
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    AutoSizeAxes = Axes.Y,
                                                    Text = YTPlayerEXStrings.DislikeCounterCredits,
                                                    Padding = new MarginPadding { Horizontal = 30, Vertical = 12 },
                                                    TextAnchor = Anchor.Centre,
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
                    CornerRadius = 12,
                    Masking = true,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4Extensions.FromHex("#1a1a1a"),
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
                                        new Container
                                        {
                                            AutoSizeAxes = Axes.X,
                                            Height = 32,
                                            CornerRadius = 12,
                                            Masking = true,
                                            AlwaysPresent = true,
                                            Children = new Drawable[]
                                            {
                                                new Container
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    CornerRadius = 12,
                                                    Child = new Box
                                                    {
                                                        RelativeSizeAxes = Axes.Both,
                                                        Colour = Color4.White,
                                                        Alpha = 0.1f,
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
                                                            Icon = FontAwesome.Solid.ThumbsUp,
                                                        },
                                                        likeCount = new AdaptiveSpriteText
                                                        {
                                                            Text = "[no metadata]"
                                                        },
                                                    }
                                                }
                                            }
                                        },
                                        new Container
                                        {
                                            AutoSizeAxes = Axes.X,
                                            Height = 32,
                                            CornerRadius = 12,
                                            Masking = true,
                                            AlwaysPresent = true,
                                            Children = new Drawable[]
                                            {
                                                new Container
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    CornerRadius = 12,
                                                    Child = new Box
                                                    {
                                                        RelativeSizeAxes = Axes.Both,
                                                        Colour = Color4.White,
                                                        Alpha = 0.1f,
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
                                                            Icon = FontAwesome.Solid.ThumbsDown,
                                                        },
                                                        dislikeCount = new AdaptiveSpriteText
                                                        {
                                                            Text = "[no metadata]"
                                                        },
                                                    }
                                                }
                                            }
                                        },
                                        commentOpenButtonDetails = new RoundedButtonContainer
                                        {
                                            AutoSizeAxes = Axes.X,
                                            Height = 32,
                                            CornerRadius = 12,
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
                                                    CornerRadius = 12,
                                                    Child = new Box
                                                    {
                                                        RelativeSizeAxes = Axes.Both,
                                                        Colour = Color4.White,
                                                        Alpha = 0.1f,
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
                                                        },
                                                        commentCount = new AdaptiveSpriteText
                                                        {
                                                            Text = "[no metadata]",
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
                                        CornerRadius = 12,
                                        Masking = true,
                                        ScrollbarVisible = false,
                                        Children = new Drawable[]
                                        {
                                            new Container
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                CornerRadius = 12,
                                                Masking = true,
                                                Child = new Box
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    Colour = Color4.White,
                                                    Alpha = 0.1f,
                                                },
                                            },
                                            new FillFlowContainer
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                CornerRadius = 12,
                                                Spacing = new Vector2(0, 8),
                                                Padding = new MarginPadding(12),
                                                Masking = true,
                                                Children = new Drawable[]
                                                {
                                                    videoInfoDetails = new AdaptiveSpriteText
                                                    {
                                                        RelativeSizeAxes = Axes.X,
                                                        Font = YouTubePlayerEXApp.DefaultFont.With(weight: "Black"),
                                                        AlwaysPresent = true,
                                                    },
                                                    videoDescription = new TextFlowContainer(f => f.Font = YouTubePlayerEXApp.DefaultFont)
                                                    {
                                                        RelativeSizeAxes = Axes.X,
                                                        AutoSizeAxes = Axes.Y,
                                                        AlwaysPresent = true,
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
                    CornerRadius = 12,
                    Masking = true,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4Extensions.FromHex("#1a1a1a"),
                        },
                        commentsContainerTitle = new AdaptiveSpriteText
                        {
                            Origin = Anchor.TopLeft,
                            Anchor = Anchor.TopLeft,
                            Text = YTPlayerEXStrings.Comments("0"),
                            Margin = new MarginPadding(16),
                            Font = YouTubePlayerEXApp.DefaultFont.With(size: 30),
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
            };

            thumbnailContainer.BlurTo(Vector2.Divide(new Vector2(10, 10), 1));
            loadVideoContainer.Hide();
            overlayFadeContainer.Hide();
            settingsContainer.Hide();
            videoDescriptionContainer.Hide();
            commentsContainer.Hide();

            hwAccelCheckbox.Current.Default = hardwareVideoDecoder.Default != HardwareVideoDecoder.None;
            hwAccelCheckbox.Current.Value = hardwareVideoDecoder.Value != HardwareVideoDecoder.None;

            hwAccelCheckbox.Current.BindValueChanged(val =>
            {
                hardwareVideoDecoder.Value = val.NewValue ? HardwareVideoDecoder.Any : HardwareVideoDecoder.None;
            });

            OverlayContainers.Add(loadVideoContainer);
            OverlayContainers.Add(settingsContainer);
            OverlayContainers.Add(videoDescriptionContainer);
            OverlayContainers.Add(commentsContainer);

            videoQuality.BindValueChanged(quality =>
            {
                //videoQualityWarning.Value = (quality.NewValue == Config.VideoQuality.Quality_8K) ? new SettingsNote.Data(YTPlayerEXStrings.VideoQuality8KWarning, SettingsNote.Type.Warning) : null;
                if (currentVideoSource != null)
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            await SetVideoSource(videoId, true);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"예외 발생: {ex.Message}");
                        }
                    });
                }
            });

            adjustPitch.BindValueChanged(value =>
            {
                currentVideoSource?.UpdatePreservePitch(value.NewValue);
            });

            audioLanguage.BindValueChanged(_ =>
            {
                if (currentVideoSource != null)
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            await SetVideoSource(videoId, true);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"예외 발생: {ex.Message}");
                        }
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
                                alert.Text = lang.NewValue != ClosedCaptionLanguage.Disabled ? (trackInfo.IsAutoGenerated ? YTPlayerEXStrings.SelectedCaptionAutoGen(lang.NewValue.GetLocalisableDescription()) : YTPlayerEXStrings.SelectedCaption(lang.NewValue.GetLocalisableDescription())) : YTPlayerEXStrings.SelectedCaption(lang.NewValue.GetLocalisableDescription());
                                alert.Show();
                                spinnerShow = Scheduler.AddDelayed(alert.Hide, 3000);
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
                    HideControls();
                }
                else
                {
                    ShowControls();
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

            videoProgress.BindValueChanged(seek =>
            {
                if (seekbar.IsDragged)
                {
                    currentVideoSource?.SeekTo(seek.NewValue * 1000);
                }
            });

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
        }

        private bool automaticRendererInUse;

        private AdaptiveSpriteText videoNameText;

        private void HideControls()
        {
            if (isControlVisible == true)
            {
                isControlVisible = false;
                uiContainer.FadeOutFromOne(250);
                uiGradientContainer.FadeOutFromOne(250);
                currentVideoSource?.UpdateControlsVisibleState(false);
            }
        }

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

        private void ShowControls()
        {
            if (isControlVisible == false)
            {
                isControlVisible = true;
                uiContainer.FadeInFromZero(125);
                uiGradientContainer.FadeInFromZero(125);
                currentVideoSource?.UpdateControlsVisibleState(true);
            }
        }

        private IBindable<bool> cursorInWindow;
        private IWindow? window;

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
        private readonly BindableBool safeAreaConsiderationsCanBeShown = new BindableBool(true);

        private FormDropdown<Size> resolutionFullscreenDropdown = null!;
        private FormDropdown<Size> resolutionWindowedDropdown = null!;
        private FormDropdown<Display> displayDropdown = null!;
        private FormDropdown<WindowMode> windowModeDropdown = null!;

        private readonly Bindable<SettingsNote.Data?> windowModeDropdownNote = new Bindable<SettingsNote.Data?>();

        private BindableNumber<double> playbackSpeed = new BindableNumber<double>(1)
        {
            MinValue = 0.5,
            MaxValue = 2,
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

        private IDisposable? duckOperation;

        private void showOverlayContainer(OverlayContainer overlayContent)
        {
            duckOperation = game.Duck(new DuckParameters
            {
                DuckVolumeTo = 1,
                DuckDuration = 100,
                RestoreDuration = 100,
            });
            overlayContent.IsVisible = true;
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
            overlayHideSample.Play();
            videoContainer?.BlurTo(new Vector2(0), 250, Easing.OutQuart);
            overlayFadeContainer.FadeTo(0f, 250, Easing.OutQuart);
            overlayContent.ScaleTo(0.8f, 250, Easing.OutQuart);
            overlayContent.FadeOutFromOne(250, Easing.OutQuart);
        }

        private bool isLoadVideoContainerVisible, isSettingsContainerVisible;

        private readonly Bindable<Display> currentDisplay = new Bindable<Display>();

        [Resolved]
        private YouTubePlayerEXAppBase app { get; set; }

        [Resolved]
        private UpdateManager updateManager { get; set; }

        private YouTubeVideoPlayer currentVideoSource;

        [Resolved]
        private GameHost host { get; set; } = null!;

        [Resolved]
        private YouTubeAPI api { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (!game.IsDeployedBuild)
                checkForUpdatesButtonCore.Hide();

            cursorInWindow?.BindValueChanged(active =>
            {
                if (active.NewValue == false)
                {
                    Schedule(() => HideControls());
                }
                else
                {
                    Schedule(() => ShowControls());
                }
            });

            loadBtn.ClickAction = async _ =>
            {
                try
                {
                    await SetVideoSource(videoIdBox.Text);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"예외 발생: {ex.Message}");
                }
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

            windowModeDropdown.Current.BindValueChanged(wndMode =>
            {
                updateDisplaySettingsVisibility();
                if (wndMode.NewValue == WindowMode.Fullscreen)
                {
                    alert.Text = YTPlayerEXStrings.FullscreenEntered;
                    alert.Show();
                    spinnerShow = Scheduler.AddDelayed(alert.Hide, 3000);
                }
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
            foreach (var item in OverlayContainers)
            {
                if (item.IsVisible == true)
                {
                    hideOverlayContainer(item);
                }
            }
        }

        private List<OverlayContainer> OverlayContainers = new List<OverlayContainer>();

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
            }

            return false;
        }

        protected override void Update()
        {
            base.Update();

            if (currentVideoSource != null)
            {
                playPause.Icon = (currentVideoSource.IsPlaying() ? FontAwesome.Solid.Pause : FontAwesome.Solid.Play);
                playPause.TooltipText = (currentVideoSource.IsPlaying() ? YTPlayerEXStrings.Pause : YTPlayerEXStrings.Play);
                videoProgress.MaxValue = currentVideoSource.VideoProgress.MaxValue;

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

        private partial class PlaybackSpeedSliderBar : RoundedSliderBar<double>
        {
            public override LocalisableString TooltipText => YTPlayerEXStrings.PlaybackSpeed(Current.Value);
        }

        private void updateVideoMetadata(string videoId)
        {
            videoMetadataDisplay.UpdateVideo(videoId);
            videoMetadataDisplayDetails.UpdateVideo(videoId);
            Task.Run(() =>
            {
                // metadata area
                Google.Apis.YouTube.v3.Data.Video videoData = api.GetVideo(videoId);

                Schedule(() => commentOpenButton.Enabled.Value = videoData.Statistics.CommentCount != null);

                commentsDisabled = videoData.Statistics.CommentCount == null;

                if (videoData.Statistics.CommentCount != null)
                    Schedule(() => commentOpenButtonDetails.Show());
                else
                    Schedule(() => commentOpenButtonDetails.Hide());

                game.RequestUpdateWindowTitle($"{videoData.Snippet.ChannelTitle} - {videoData.Snippet.Title}");

                DateTimeOffset? dateTime = videoData.Snippet.PublishedAtDateTimeOffset;
                DateTime now = DateTime.Now;
                Schedule(() => thumbnailContainer.SetImageUrl(videoData.Snippet.Thumbnails.High.Url));
                Schedule(() => videoDescription.Text = api.GetLocalizedVideoDescription(videoData));
                commentCount.Text = videoData.Statistics.CommentCount != null ? Convert.ToInt32(videoData.Statistics.CommentCount).ToStandardFormattedString(0) : YTPlayerEXStrings.DisabledByUploader;
                dislikeCount.Text = videoData.Statistics.DislikeCount != null ? Convert.ToInt32(videoData.Statistics.DislikeCount).ToStandardFormattedString(0) : ReturnYouTubeDislike.GetDislikes(videoId).Dislikes.ToStandardFormattedString(0);
                likeCount.Text = videoData.Statistics.LikeCount != null ? Convert.ToInt32(videoData.Statistics.LikeCount).ToStandardFormattedString(0) : ReturnYouTubeDislike.GetDislikes(videoId).RawLikes.ToStandardFormattedString(0);
                commentsContainerTitle.Text = YTPlayerEXStrings.Comments(videoData.Statistics.CommentCount != null ? Convert.ToInt32(videoData.Statistics.CommentCount).ToStandardFormattedString(0) : YTPlayerEXStrings.Disabled);
                videoInfoDetails.Text = YTPlayerEXStrings.VideoMetadataDescWithoutChannelName(Convert.ToInt32(videoData.Statistics.ViewCount).ToStandardFormattedString(0), dateTime.Value.DateTime.Humanize(dateToCompareAgainst: now));

                // comments area
                IList<CommentThread> commentThreadData = api.GetCommentThread(videoId);
                foreach (CommentThread item in commentThreadData)
                {
                    if (item.Snippet.IsPublic == true)
                    {
                        Schedule(() =>
                        {
                            commentContainer.Add(new CommentDisplay(api.GetComment(item.Id))
                            {
                                RelativeSizeAxes = Axes.X,
                            });
                        });
                    }
                }

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
            currentVideoSource?.Expire();
            if (loadVideoContainer.IsVisible == true)
            {
                Schedule(() => hideOverlayContainer(loadVideoContainer));
            }
            videoIdBox.Text = string.Empty;

            foreach (var item in commentContainer.Children)
            {
                item.Expire();
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
                    alert.Text = YTPlayerEXStrings.CannotPlayPrivateVideos;
                    alert.Show();
                    spinnerShow = Scheduler.AddDelayed(alert.Hide, 3000);
                    return;
                }

                IProgress<double> audioDownloadProgress = new Progress<double>((percent) => videoLoadingProgress.Text = $"Downloading audio cache: {(percent * 100):N0}%");
                IProgress<double> videoDownloadProgress = new Progress<double>((percent) => videoLoadingProgress.Text = $"Downloading video cache: {(percent * 100):N0}%");

                spinnerShow = Scheduler.AddDelayed(spinner.Show, 0);

                videoProgress.MaxValue = 1;
                videoUrl = $"https://youtube.com/watch?v={videoId}";

                spinnerShow = Scheduler.AddDelayed(() => updateVideoMetadata(videoId), 0);

                if (!File.Exists(app.Host.CacheStorage.GetStorageForDirectory("videos").GetFullPath($"{videoId}") + @"\audio.mp3") || !File.Exists(app.Host.CacheStorage.GetStorageForDirectory("videos").GetFullPath($"{videoId}") + @"\video.mp4"))
                {
                    videoQuality.Disabled = audioLanguage.Disabled = true;

                    Directory.CreateDirectory(app.Host.CacheStorage.GetStorageForDirectory("videos").GetFullPath($"{videoId}"));

                    var streamManifest = await app.YouTubeClient.Videos.Streams.GetManifestAsync(videoUrl);

                    IStreamInfo audioStreamInfo;

                    try
                    {
                        // Select best audio stream (highest bitrate)
                        audioStreamInfo = streamManifest
                            .GetAudioOnlyStreams()
                            .Where(s => s.AudioLanguage.Value.Code == appGlobalConfig.Get<Language>(YTPlayerEXSetting.AudioLanguage).ToString()) //Fix black screen on some videos
                            .GetWithHighestBitrate();
                    }
                    catch (Exception e)
                    {
                        // Select best audio stream (highest bitrate)
                        audioStreamInfo = streamManifest
                            .GetAudioOnlyStreams()
                            .GetWithHighestBitrate();
                    }

                    IVideoStreamInfo videoStreamInfo;

                    if (videoQuality.Value == Config.VideoQuality.PreferHighQuality)
                    {
                        // Select best video stream (1080p60 in this example)
                        videoStreamInfo = streamManifest
                            .GetVideoOnlyStreams()
                            .Where(s => s.Container == YoutubeExplode.Videos.Streams.Container.Mp4)
                            .Where(s => s.VideoCodec.Contains("avc1")) //Fix black screen on some videos
                            .GetWithHighestVideoQuality();
                    }
                    else
                    {
                        // Select best video stream (1080p60 in this example)
                        videoStreamInfo = streamManifest
                            .GetVideoOnlyStreams()
                            .Where(s => s.Container == YoutubeExplode.Videos.Streams.Container.Mp4)
                            .Where(s => s.VideoCodec.Contains("avc1")) //Fix black screen on some videos
                            .Where(s => s.VideoQuality.Label.Contains(app.ParseVideoQuality()))
                            .GetWithHighestVideoQuality();
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
                                alert.Text = captionLanguage.Value != ClosedCaptionLanguage.Disabled ? (trackInfo.IsAutoGenerated ? YTPlayerEXStrings.SelectedCaptionAutoGen(captionLanguage.Value.GetLocalisableDescription()) : YTPlayerEXStrings.SelectedCaption(captionLanguage.Value.GetLocalisableDescription())) : YTPlayerEXStrings.SelectedCaption(captionLanguage.Value.GetLocalisableDescription());
                                alert.Show();
                                spinnerShow = Scheduler.AddDelayed(alert.Hide, 3000);
                            });
                        }

                        captionTrack = await game.YouTubeClient.Videos.ClosedCaptions.GetAsync(trackInfo);
                    }

                    await app.YouTubeClient.Videos.DownloadAsync([audioStreamInfo], new ConversionRequestBuilder(app.Host.CacheStorage.GetStorageForDirectory("videos").GetFullPath($"{videoId}") + @"\audio.mp3").Build(), audioDownloadProgress);
                    await app.YouTubeClient.Videos.DownloadAsync([videoStreamInfo], new ConversionRequestBuilder(app.Host.CacheStorage.GetStorageForDirectory("videos").GetFullPath($"{videoId}") + @"\video.mp4").Build(), videoDownloadProgress);

                    currentVideoSource = new YouTubeVideoPlayer(app.Host.CacheStorage.GetStorageForDirectory("videos").GetFullPath($"{videoId}") + @"\video.mp4", app.Host.CacheStorage.GetStorageForDirectory("videos").GetFullPath($"{videoId}") + @"\audio.mp3", captionTrack, captionLanguage.Value, pausedTime)
                    {
                        RelativeSizeAxes = Axes.Both
                    };

                    spinnerShow = Scheduler.AddDelayed(spinner.Hide, 0);

                    spinnerShow = Scheduler.AddDelayed(addVideoToScreen, 0);

                    spinnerShow = Scheduler.AddDelayed(() => playVideo(), 1000);

                    videoQuality.Disabled = audioLanguage.Disabled = false;
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
                                alert.Text = captionLanguage.Value != ClosedCaptionLanguage.Disabled ? (trackInfo.IsAutoGenerated ? YTPlayerEXStrings.SelectedCaptionAutoGen(captionLanguage.Value.GetLocalisableDescription()) : YTPlayerEXStrings.SelectedCaption(captionLanguage.Value.GetLocalisableDescription())) : YTPlayerEXStrings.SelectedCaption(captionLanguage.Value.GetLocalisableDescription());
                                alert.Show();
                                spinnerShow = Scheduler.AddDelayed(alert.Hide, 3000);
                            });
                        }

                        captionTrack = await game.YouTubeClient.Videos.ClosedCaptions.GetAsync(trackInfo);
                    }

                    currentVideoSource = new YouTubeVideoPlayer(app.Host.CacheStorage.GetStorageForDirectory("videos").GetFullPath($"{videoId}") + @"\video.mp4", app.Host.CacheStorage.GetStorageForDirectory("videos").GetFullPath($"{videoId}") + @"\audio.mp3", captionTrack, captionLanguage.Value, pausedTime)
                    {
                        RelativeSizeAxes = Axes.Both
                    };

                    spinnerShow = Scheduler.AddDelayed(spinner.Hide, 0);

                    spinnerShow = Scheduler.AddDelayed(addVideoToScreen, 0);

                    spinnerShow = Scheduler.AddDelayed(() => playVideo(), 1000);
                }
            }
            else
            {
                alert.Text = YTPlayerEXStrings.NoVideoIdError;
                alert.Show();
                spinnerShow = Scheduler.AddDelayed(alert.Hide, 3000);
            }
        }

        private Storage exportStorage = null!;

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
                alert.Text = YTPlayerEXStrings.LogsExportFinished;
                alert.Show();
                spinnerShow = Scheduler.AddDelayed(alert.Hide, 3000);
                exportStorage.PresentFileExternally(archive_filename);
            });
        }

        [Resolved]
        private YouTubePlayerEXApp game { get; set; }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

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
                    return LocalisableString.Interpolate($"{base.GenerateItemText(item)} ({hostResolvedRenderer.GetDescription()})");

                return base.GenerateItemText(item);
            }
        }
    }
}
