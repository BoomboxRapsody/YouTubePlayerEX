// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using YouTubePlayerEX.App.Localisation;

namespace YouTubePlayerEX.App.Overlays.OSD
{
    public partial class SpeedChangeToast : Toast
    {
        public SpeedChangeToast(double newSpeed)
            : base(YTPlayerEXStrings.PlaybackSpeedWithoutValue, $@"{newSpeed:0.##}x")
        {
        }
    }
}
