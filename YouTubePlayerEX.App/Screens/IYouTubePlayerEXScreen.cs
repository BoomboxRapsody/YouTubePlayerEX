// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Screens;

namespace YouTubePlayerEX.App.Screens
{
    public interface IYouTubePlayerEXScreen : IScreen
    {
        /// <summary>
        /// Whether this <see cref="YouTubePlayerEXScreen"/> allows the cursor to be displayed.
        /// </summary>
        bool CursorVisible { get; }
    }
}
