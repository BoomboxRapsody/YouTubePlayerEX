// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using YouTubePlayerEX.App.Localisation;

namespace YouTubePlayerEX.App.Config
{
    public enum ScreenshotFormat
    {
        [LocalisableDescription(typeof(YTPlayerEXStrings), nameof(YTPlayerEXStrings.Jpg))]
        Jpg = 1,

        [LocalisableDescription(typeof(YTPlayerEXStrings), nameof(YTPlayerEXStrings.Png))]
        Png = 2
    }
}
