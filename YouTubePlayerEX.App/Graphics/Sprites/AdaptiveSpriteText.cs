using System;
using osu.Framework.Graphics.Sprites;

namespace YouTubePlayerEX.App.Graphics.Sprites
{
    public partial class AdaptiveSpriteText : SpriteText
    {
        [Obsolete("Use TruncatingSpriteText instead.")]
        public new bool Truncate
        {
            set => throw new InvalidOperationException($"Use {nameof(TruncatingSpriteText)} instead.");
        }

        public AdaptiveSpriteText(bool enableShadow = true)
        {
            Font = YouTubePlayerEXApp.DefaultFont;
            Shadow = enableShadow;
        }
    }
}
