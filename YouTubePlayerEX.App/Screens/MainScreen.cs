using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osuTK;
using osuTK.Graphics;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;
using YouTubePlayerEX.App.Graphics.Sprites;
using YouTubePlayerEX.App.Graphics.UserInterface;
using YouTubePlayerEX.App.Graphics.Videos;
using YouTubePlayerEX.App.Input;
using YouTubePlayerEX.App.Input.Binding;
using YouTubePlayerEX.App.Localisation;
using Container = osu.Framework.Graphics.Containers.Container;
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
        private OverlayContainer loadVideoContainer, settingsContainer;
        private AdaptiveButton loadBtnOverlayShow;
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

        [BackgroundDependencyLoader]
        private void load(ISampleStore sampleStore)
        {
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
                            Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0.5f), Color4.Black.Opacity(0)),
                            Origin = Anchor.TopLeft,
                            Anchor = Anchor.TopLeft,
                            RelativeSizeAxes = Axes.X,
                            Height = 150,
                        },
                        new Box
                        {
                            Colour = ColourInfo.GradientVertical(Color4.Black.Opacity(0), Color4.Black.Opacity(0.5f)),
                            Origin = Anchor.BottomLeft,
                            Anchor = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.X,
                            Height = 150,
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
                                        new RoundedSliderBar<double>
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            PlaySamplesOnAdjust = false,
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
                                                    KeyboardStep = 0.25f,
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
                overlayFadeContainer = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
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
            };

            loadVideoContainer.Hide();
            overlayFadeContainer.Hide();

            idleTracker.IsIdle.BindValueChanged(idle =>
            {
                if (idle.NewValue == true)
                {
                    uiContainer.FadeOutFromOne(250);
                    uiGradientContainer.FadeOutFromOne(250);
                } else
                {
                    uiContainer.FadeInFromZero(125);
                    uiGradientContainer.FadeInFromZero(125);
                }
            }, true);
        }

        private BindableNumber<double> playbackSpeed = new BindableNumber<double>(1)
        {
            MinValue = 0.5,
            MaxValue = 2,
        };

        private void showLoadVideoContainer()
        {
            isLoadVideoContainerVisible = true;
            overlayFadeContainer.FadeTo(0.5f, 250, Easing.OutQuart);
            loadVideoContainer.Show();
            loadVideoContainer.ScaleTo(0.8f);
            loadVideoContainer.ScaleTo(1f, 750, Easing.OutElastic);
            loadVideoContainer.FadeInFromZero(250, Easing.OutQuart);
            overlayShowSample.Play();
        }

        private void hideLoadVideoContainer()
        {
            isLoadVideoContainerVisible = false;
            overlayHideSample.Play();
            overlayFadeContainer.FadeTo(0.0f, 250, Easing.OutQuart);
            loadVideoContainer.ScaleTo(0.8f, 250, Easing.OutQuart);
            loadVideoContainer.FadeOutFromOne(250, Easing.OutQuart).OnComplete(_ =>
            {
                overlayFadeContainer.Hide();
                loadVideoContainer.Hide();
            });
        }

        private bool isLoadVideoContainerVisible;

        [Resolved]
        private YouTubePlayerEXAppBase app { get; set; }

        private YouTubeVideoPlayer currentVideoSource;

        protected override void LoadComplete()
        {
            base.LoadComplete();
            loadBtn.ClickAction = _ => SetVideoSource(videoIdBox.Text);
            loadBtnOverlayShow.ClickAction = _ => showLoadVideoContainer();
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            switch (e.Action)
            {
                case GlobalAction.Back:
                    if (isLoadVideoContainerVisible)
                    {
                        hideLoadVideoContainer();
                    }
                    return true;

                case GlobalAction.Select:
                    if (isLoadVideoContainerVisible)
                    {
                        SetVideoSource(videoIdBox.Text);
                    }
                    return true;

                case GlobalAction.FastForward_10sec:
                    currentVideoSource?.FastForward10Sec();
                    return true;

                case GlobalAction.FastRewind_10sec:
                    currentVideoSource?.FastRewind10Sec();
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
            videoLoadingProgress.Text = "";
        }

        private void addVideoToScreen()
        {
            videoContainer.Add(currentVideoSource);

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

        public void SetVideoSource(string videoId)
        {
            hideLoadVideoContainer();
            if (videoId.Length != 0)
            {
                Task.Run(async () =>
                {
                    IProgress<double> audioDownloadProgress = new Progress<double>((percent) => videoLoadingProgress.Text = $"Downloading audio cache: {(percent * 100):N0}%");
                    IProgress<double> videoDownloadProgress = new Progress<double>((percent) => videoLoadingProgress.Text = $"Downloading video cache: {(percent * 100):N0}%");

                    spinnerShow = Scheduler.AddDelayed(spinner.Show, 0);

                    videoProgress.MaxValue = 1;
                    currentVideoSource?.Expire();
                    var videoUrl = $"https://youtube.com/watch?v={videoId}";

                    if (!File.Exists(app.Host.CacheStorage.GetStorageForDirectory("videos").GetFullPath($"{videoId}") + @"\audio.mp3") && !File.Exists(app.Host.CacheStorage.GetStorageForDirectory("videos").GetFullPath($"{videoId}") + @"\video.mp4"))
                    {
                        Directory.CreateDirectory(app.Host.CacheStorage.GetStorageForDirectory("videos").GetFullPath($"{videoId}"));

                        var streamManifest = await app.YouTubeClient.Videos.Streams.GetManifestAsync(videoUrl);

                        // Select best audio stream (highest bitrate)
                        var audioStreamInfo = streamManifest
                            .GetAudioOnlyStreams()
                            .GetWithHighestBitrate();

                        // Select best video stream (1080p60 in this example)
                        var videoStreamInfo = streamManifest
                            .GetVideoOnlyStreams()
                            .Where(s => s.Container == YoutubeExplode.Videos.Streams.Container.WebM)
                            .GetWithHighestVideoQuality();

                        await app.YouTubeClient.Videos.DownloadAsync([audioStreamInfo], new ConversionRequestBuilder(app.Host.CacheStorage.GetStorageForDirectory("videos").GetFullPath($"{videoId}") + @"\audio.mp3").Build(), audioDownloadProgress);
                        await app.YouTubeClient.Videos.DownloadAsync([videoStreamInfo], new ConversionRequestBuilder(app.Host.CacheStorage.GetStorageForDirectory("videos").GetFullPath($"{videoId}") + @"\video.mp4").Build(), videoDownloadProgress);

                        currentVideoSource = new YouTubeVideoPlayer(app.Host.CacheStorage.GetStorageForDirectory("videos").GetFullPath($"{videoId}") + @"\video.mp4", app.Host.CacheStorage.GetStorageForDirectory("videos").GetFullPath($"{videoId}") + @"\audio.mp3")
                        {
                            RelativeSizeAxes = Axes.Both
                        };

                        spinnerShow = Scheduler.AddDelayed(spinner.Hide, 0);

                        spinnerShow = Scheduler.AddDelayed(addVideoToScreen, 0);

                        spinnerShow = Scheduler.AddDelayed(() => updateVideoMetadata(videoId), 0);

                        spinnerShow = Scheduler.AddDelayed(() => playVideo(), 1000);
                    }
                    else
                    {
                        currentVideoSource = new YouTubeVideoPlayer(app.Host.CacheStorage.GetStorageForDirectory("videos").GetFullPath($"{videoId}") + @"\video.mp4", app.Host.CacheStorage.GetStorageForDirectory("videos").GetFullPath($"{videoId}") + @"\audio.mp3")
                        {
                            RelativeSizeAxes = Axes.Both
                        };

                        spinnerShow = Scheduler.AddDelayed(spinner.Hide, 0);

                        spinnerShow = Scheduler.AddDelayed(addVideoToScreen, 0);

                        spinnerShow = Scheduler.AddDelayed(() => updateVideoMetadata(videoId), 0);

                        spinnerShow = Scheduler.AddDelayed(() => playVideo(), 1000);
                    }
                });
            }
            else
            {
                alert.Text = YTPlayerEXStrings.NoVideoIdError;
                alert.Show();
                spinnerShow = Scheduler.AddDelayed(alert.Hide, 3000);
            }
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }
    }
}
