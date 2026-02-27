// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Video;
using osu.Framework.Timing;

namespace YouTubePlayerEX.App.Graphics.Videos
{
    public partial class IntroScreenVideo : Container
    {
        private Video video = null!;
        private Track track = null!;

        private DrawableTrack drawableTrack = null!;

        private StopwatchClock rateAdjustClock = null!;
        private DecouplingFramedClock framedClock = null!;

        public Action TrackCompleted;

        [BackgroundDependencyLoader]
        private void load(ITrackStore tracks)
        {
            track = tracks.Get(@"intro");

            rateAdjustClock = new StopwatchClock(false);
            framedClock = new DecouplingFramedClock(rateAdjustClock);

            AddRange(new Drawable[] {
                drawableTrack = new DrawableTrack(track)
                {
                    Clock = framedClock,
                },
                video = new Video(Directory.GetCurrentDirectory() + "/intro.mp4", false)
                {
                    RelativeSizeAxes = Axes.Both,
                    FillMode = FillMode.Fill,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Clock = framedClock,
                }
            });

            drawableTrack.Completed += trackCompleted;

            drawableTrack.Start();
            framedClock.Start();
        }

        private void trackCompleted()
        {
            TrackCompleted?.Invoke();
        }
    }
}
