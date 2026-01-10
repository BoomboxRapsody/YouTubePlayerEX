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
