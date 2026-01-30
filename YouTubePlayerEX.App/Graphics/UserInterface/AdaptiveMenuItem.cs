// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;

namespace YouTubePlayerEX.App.Graphics.UserInterface
{
    public class AdaptiveMenuItem : MenuItem
    {
        public IconUsage Icon { get; init; }

        public AdaptiveMenuItem(LocalisableString text)
            : this(text, null)
        {
        }

        public AdaptiveMenuItem(LocalisableString text, Action? action)
            : base(text, action)
        {
        }
    }
}
