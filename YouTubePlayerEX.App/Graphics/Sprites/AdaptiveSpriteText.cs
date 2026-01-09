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

        public AdaptiveSpriteText()
        {
            Font = YouTubePlayerEXApp.DefaultFont;
            Shadow = true;
        }
    }
}
