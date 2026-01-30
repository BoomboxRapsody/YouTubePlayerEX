// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Screens;

namespace YouTubePlayerEX.App.Screens
{
    public abstract partial class YouTubePlayerEXScreen : Screen
    {
        protected new YouTubePlayerEXAppBase Game => base.Game as YouTubePlayerEXAppBase;
    }
}
