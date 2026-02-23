// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Threading;
using osuTK;
using osuTK.Graphics;
using YouTubePlayerEX.App.Graphics;
using YouTubePlayerEX.App.Graphics.UserInterface;
using YouTubePlayerEX.App.Input.Binding;
using YouTubePlayerEX.App.Localisation;
using YouTubePlayerEX.App.Overlays.Volume;

namespace YouTubePlayerEX.App.Overlays
{
    [Cached]
    public partial class VolumeOverlay : VisibilityContainer
    {
        public Bindable<bool> IsMuted { get; } = new Bindable<bool>();

        private const float offset = 10;

        private VolumeMeter volumeMeterMaster = null!;
        private VolumeMeter volumeMeterEffect = null!;
        private VolumeMeter volumeMeterMusic = null!;

        private SelectionCycleFillFlowContainer<VolumeMeter> volumeMeters = null!;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, AdaptiveColour colours)
        {
            AutoSizeAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;

            AddRange(new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 300,
                    Colour = ColourInfo.GradientHorizontal(Color4.Black.Opacity(0.75f), Color4.Black.Opacity(0))
                },
                new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Spacing = new Vector2(0, offset),
                    Margin = new MarginPadding { Left = offset },
                    Children = new Drawable[]
                    {
                        volumeMeters = new SelectionCycleFillFlowContainer<VolumeMeter>
                        {
                            Direction = FillDirection.Vertical,
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Spacing = new Vector2(0, offset),
                            Children = new[]
                            {
                                volumeMeterEffect = new VolumeMeter(YTPlayerEXStrings.SFXVolume, 125, colours.BlueDarker),
                                volumeMeterMaster = new MasterVolumeMeter(YTPlayerEXStrings.MasterVolume, 150, colours.PinkDarker) { IsMuted = { BindTarget = IsMuted }, },
                                volumeMeterMusic = new VolumeMeter(YTPlayerEXStrings.VideoVolume, 125, colours.BlueDarker),
                            }
                        },
                    },
                },
            });

            volumeMeterMaster.Bindable.BindTo(audio.Volume);
            volumeMeterEffect.Bindable.BindTo(audio.VolumeSample);
            volumeMeterMusic.Bindable.BindTo(audio.VolumeTrack);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            foreach (var volumeMeter in volumeMeters)
                volumeMeter.Bindable.ValueChanged += _ => Show();
        }

        public bool Adjust(GlobalAction action, float amount = 1, bool isPrecise = false)
        {
            if (!IsLoaded) return false;

            switch (action)
            {
                case GlobalAction.DecreaseVolume:
                    if (State.Value == Visibility.Hidden)
                        Show();
                    else
                        volumeMeters.Selected?.Decrease(amount, isPrecise);
                    return true;

                case GlobalAction.IncreaseVolume:
                    if (State.Value == Visibility.Hidden)
                        Show();
                    else
                        volumeMeters.Selected?.Increase(amount, isPrecise);
                    return true;

                case GlobalAction.NextVolumeMeter:
                    if (State.Value != Visibility.Visible)
                        return false;

                    volumeMeters.SelectNext();
                    Show();
                    return true;

                case GlobalAction.PreviousVolumeMeter:
                    if (State.Value != Visibility.Visible)
                        return false;

                    volumeMeters.SelectPrevious();
                    Show();
                    return true;

                case GlobalAction.ToggleMute:
                    Show();
                    volumeMeters.OfType<MasterVolumeMeter>().First().ToggleMute();
                    return true;
            }

            return false;
        }

        public void FocusMasterVolume()
        {
            volumeMeters.Select(volumeMeterMaster);
        }

        public override void Show()
        {
            // Focus on the master meter as a default if previously hidden
            if (State.Value == Visibility.Hidden)
                FocusMasterVolume();

            if (State.Value == Visibility.Visible)
                schedulePopOut();

            base.Show();
        }

        protected override void PopIn()
        {
            ClearTransforms();
            this.FadeIn(100);

            schedulePopOut();
        }

        protected override void PopOut()
        {
            this.FadeOut(100);
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            // keep the scheduled event correctly timed as long as we have movement.
            schedulePopOut();
            return base.OnMouseMove(e);
        }

        protected override bool OnHover(HoverEvent e)
        {
            schedulePopOut();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            schedulePopOut();
            base.OnHoverLost(e);
        }

        private ScheduledDelegate? popOutDelegate;

        private void schedulePopOut()
        {
            popOutDelegate?.Cancel();
            this.Delay(1000).Schedule(() =>
            {
                if (!IsHovered)
                    Hide();
            }, out popOutDelegate);
        }
    }
}
