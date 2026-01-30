// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using YouTubePlayerEX.App.Graphics.UserInterface;

namespace YouTubePlayerEX.App.Config
{
    /// <summary>
    /// Stores global per-session statics. These will not be stored after exiting the game.
    /// </summary>
    public class SessionStatics : InMemoryConfigManager<Static>
    {
        protected override void InitialiseDefaults()
        {
            SetDefault(Static.LastHoverSoundPlaybackTime, (double?)null);
        }
    }

    public enum Static
    {
        /// <summary>
        /// The last playback time in milliseconds of a hover sample (from <see cref="HoverSounds"/>).
        /// </summary>
        LastHoverSoundPlaybackTime,
    }
}
