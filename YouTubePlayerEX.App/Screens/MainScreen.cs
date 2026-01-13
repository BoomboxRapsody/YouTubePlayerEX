using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Video;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Framework.Platform.Windows;
using osu.Framework.Threading;
using osuTK;
using osuTK.Graphics;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.ClosedCaptions;
using YoutubeExplode.Videos.Streams;
using YouTubePlayerEX.App.Config;
using YouTubePlayerEX.App.Graphics.Containers;
using YouTubePlayerEX.App.Graphics.Sprites;
using YouTubePlayerEX.App.Graphics.UserInterface;
using YouTubePlayerEX.App.Graphics.UserInterfaceV2;
using YouTubePlayerEX.App.Graphics.Videos;
using YouTubePlayerEX.App.Input;
using YouTubePlayerEX.App.Input.Binding;
using YouTubePlayerEX.App.Localisation;
using YouTubePlayerEX.App.Online;
using Container = osu.Framework.Graphics.Containers.Container;
using Language = YouTubePlayerEX.App.Localisation.Language;
using OverlayContainer = YouTubePlayerEX.App.Graphics.Containers.OverlayContainer;

namespace YouTubePlayerEX.App.Screens
{
    public partial class MainScreen : YouTubePlayerEXScreen, IKeyBindingHandler<GlobalAction>
    {
        private Container videoContainer;
        private AdaptiveButton loadBtn;
        private AdaptiveTextBox videoIdBox;
        private LoadingSpinner spinner;
        private ScheduledDelegate spinnerShow;
        private AdaptiveAlertContainer alert;
        private IdleTracker idleTracker;
        private Container uiContainer;
        private Container uiGradientContainer;
        private OverlayContainer loadVideoContainer, settingsContainer, videoDescriptionContainer;
        private AdaptiveButton loadBtnOverlayShow, settingsOverlayShowBtn;
        private VideoMetadataDisplay videoMetadataDisplay;

        private Sample overlayShowSample;
        private Sample overlayHideSample;

        private Box overlayFadeContainer;

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            // Be sure to dispose the track, otherwise memory will be leaked!
            // This is automatic for DrawableTrack.
            overlayShowSample.Dispose();
            overlayHideSample.Dispose();
        }

        private AdaptiveSpriteText videoLoadingProgress;

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

        [BackgroundDependencyLoader]
        private void load(ISampleStore sampleStore, FrameworkConfigManager config, YTPlayerEXConfigManager appConfig, GameHost host)
        {
            window = host.Window;

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
                videoContainer = new Container
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
                        videoMetadataDisplay = new VideoMetadataDisplay
                        {
                            Width = 400,
                            Height = 60,
                            Origin = Anchor.TopLeft,
                            Anchor = Anchor.TopLeft,
                        },
                        loadBtnOverlayShow = new IconButton
                        {
                            Enabled = { Value = true },
                            Origin = Anchor.TopRight,
                            Anchor = Anchor.TopRight,
                            Size = new Vector2(40, 40),
                            Icon = FontAwesome.Regular.FolderOpen,
                            IconScale = new Vector2(1.2f),
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
                        },
                        alert = new AdaptiveAlertContainer
                        {
                            Origin = Anchor.TopCentre,
                            Anchor = Anchor.TopCentre,
                            Size = new Vector2(600, 60),
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
                                        new RoundedSliderBarWithoutTooltip
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            PlaySamplesOnAdjust = false,
                                            DisplayAsPercentage = true,
                                            Current = { BindTarget = videoProgress },
                                            TransferValueOnCommit = true,
                                        },
                                        new Container
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Children = new Drawable[] {
                                                new AdaptiveSpriteText
                                                {
                                                    Anchor = Anchor.TopLeft,
                                                    Origin = Anchor.TopLeft,
                                                    Text = "0:00",
                                                },
                                                new AdaptiveSpriteText
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
                                                new IconButton
                                                {
                                                    Enabled = { Value = true },
                                                    Icon = FontAwesome.Solid.FastBackward,
                                                },
                                                playPause = new IconButton
                                                {
                                                    Enabled = { Value = true },
                                                    Icon = FontAwesome.Solid.Play,
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
                                                new IconButton
                                                {
                                                    Enabled = { Value = true },
                                                    Icon = FontAwesome.Solid.FastForward,
                                                },
                                                new RoundedSliderBar<double>
                                                {
                                                    Width = 200,
                                                    Margin = new MarginPadding
                                                    {
                                                        Top = 8,
                                                    },
                                                    KeyboardStep = 0.05f,
                                                    PlaySamplesOnAdjust = true,
                                                    Current = { BindTarget = playbackSpeed },
                                                    DisplayAsPercentage = true,
                                                },
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
                    Colour = Color4.Black,
                    ClickAction = _ => hideOverlays(),
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
                                                new SettingsItemV2(new FormSliderBar<float>
                                                {
                                                    Caption = YTPlayerEXStrings.UIScaling,
                                                    TransferValueOnCommit = true,
                                                    Current = appConfig.GetBindable<float>(YTPlayerEXSetting.UIScale),
                                                    KeyboardStep = 0.01f,
                                                    LabelFormat = v => $@"{v:0.##}x",
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
                        videoNameText = new AdaptiveSpriteText
                        {
                            Origin = Anchor.TopLeft,
                            Anchor = Anchor.TopLeft,
                            Text = YTPlayerEXStrings.LoadFromVideoId,
                            Margin = new MarginPadding(16),
                            Font = YouTubePlayerEXApp.DefaultFont.With(size: 30),
                        },
                    }
                },
            };

            loadVideoContainer.Hide();
            overlayFadeContainer.Hide();
            settingsContainer.Hide();
            videoDescriptionContainer.Hide();

            hwAccelCheckbox.Current.Default = hardwareVideoDecoder.Default != HardwareVideoDecoder.None;
            hwAccelCheckbox.Current.Value = hardwareVideoDecoder.Value != HardwareVideoDecoder.None;

            hwAccelCheckbox.Current.BindValueChanged(val =>
            {
                hardwareVideoDecoder.Value = val.NewValue ? HardwareVideoDecoder.Any : HardwareVideoDecoder.None;
            });

            OverlayContainers.Add(loadVideoContainer);
            OverlayContainers.Add(settingsContainer);
            OverlayContainers.Add(videoDescriptionContainer);

            videoQuality.BindValueChanged(_ =>
            {
                if (currentVideoSource != null)
                {
                    Task.Run(async () => {
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
                    Task.Run(async () => {
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
        }

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

        private SettingsItemV2 resolutionFullscreenDropdownCore, resolutionWindowedDropdownCore, displayDropdownCore, minimiseOnFocusLossCheckboxCore;

        private FormCheckBox hwAccelCheckbox;
        private FormEnumDropdown<Config.VideoQuality> videoQualitySettings;

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
                    return "Default";

                return $"{item.Width}x{item.Height}";
            }
        }

        private void showOverlayContainer(OverlayContainer overlayContent)
        {
            overlayContent.IsVisible = true;
            overlayFadeContainer.FadeTo(0.5f, 250, Easing.OutQuart);
            overlayContent.Show();
            overlayContent.ScaleTo(0.8f);
            overlayContent.ScaleTo(1f, 750, Easing.OutElastic);
            overlayContent.FadeInFromZero(250, Easing.OutQuart);
            overlayShowSample.Play();
        }

        private void hideOverlayContainer(OverlayContainer overlayContent)
        {
            overlayContent.IsVisible = false;
            overlayHideSample.Play();
            overlayFadeContainer.FadeTo(0.0f, 250, Easing.OutQuart);
            overlayContent.ScaleTo(0.8f, 250, Easing.OutQuart);
            overlayContent.FadeOutFromOne(250, Easing.OutQuart).OnComplete(_ =>
            {
                overlayFadeContainer.Hide();
                overlayContent.Hide();
            });
        }

        private bool isLoadVideoContainerVisible, isSettingsContainerVisible;

        private readonly Bindable<Display> currentDisplay = new Bindable<Display>();

        [Resolved]
        private YouTubePlayerEXAppBase app { get; set; }

        private YouTubeVideoPlayer currentVideoSource;

        [Resolved]
        private GameHost host { get; set; } = null!;

        [Resolved]
        private YouTubeAPI api { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

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
                            currentVideoSource.Pause();
                        else
                            currentVideoSource.Play();
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
                videoProgress.MaxValue = currentVideoSource.VideoProgress.MaxValue;

                if (currentVideoSource.IsPlaying())
                {
                    videoProgress.Value = currentVideoSource.VideoProgress.Value;
                }
            }
        }

        private IconButton playPause;

        private void updateVideoMetadata(string videoId)
        {
            videoMetadataDisplay.UpdateVideo(videoId);
            Task.Run(() =>
            {
                videoNameText.Text = api.GetLocalizedVideoTitle(api.GetVideo(videoId));
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
                    } catch (Exception e)
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

        [Resolved]
        private YouTubePlayerEXAppBase game { get; set; }

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
    }
}
