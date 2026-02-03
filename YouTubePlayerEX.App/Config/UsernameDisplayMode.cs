// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using YouTubePlayerEX.App.Localisation;

namespace YouTubePlayerEX.App.Config
{
    public enum UsernameDisplayMode
    {
        [LocalisableDescription(typeof(YTPlayerEXStrings), nameof(YTPlayerEXStrings.UsernameDisplayMode_DisplayName))]
        DisplayName,

        [LocalisableDescription(typeof(YTPlayerEXStrings), nameof(YTPlayerEXStrings.UsernameDisplayMode_Handle))]
        Handle,
    }
}
