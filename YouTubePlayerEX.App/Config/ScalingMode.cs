// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Localisation;
using YouTubePlayerEX.App.Localisation;

namespace YouTubePlayerEX.App.Config
{
    public enum ScalingMode
    {
        [LocalisableDescription(typeof(YTPlayerEXStrings), nameof(YTPlayerEXStrings.ScalingOff))]
        Off,

        [LocalisableDescription(typeof(YTPlayerEXStrings), nameof(YTPlayerEXStrings.ScaleEverything))]
        Everything,

        [LocalisableDescription(typeof(YTPlayerEXStrings), nameof(YTPlayerEXStrings.ScaleVideo))]
        Video,
    }
}
