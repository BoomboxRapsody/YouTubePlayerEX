// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.UserInterface;

namespace YouTubePlayerEX.App.Graphics.UserInterfaceV2
{
    public partial class AdaptiveColourPicker : ColourPicker
    {
        public AdaptiveColourPicker()
        {
            CornerRadius = 10;
            Masking = true;
        }

        protected override HSVColourPicker CreateHSVColourPicker() => new AdaptiveHSVColourPicker();
        protected override HexColourPicker CreateHexColourPicker() => new AdaptiveHexColourPicker();
    }
}
