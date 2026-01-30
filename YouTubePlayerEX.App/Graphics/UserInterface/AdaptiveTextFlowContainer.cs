// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using YouTubePlayerEX.App.Graphics.Sprites;

namespace YouTubePlayerEX.App.Graphics.UserInterface
{
    public partial class AdaptiveTextFlowContainer : TextFlowContainer
    {
        public AdaptiveTextFlowContainer(Action<SpriteText>? defaultCreationParameters = null)
            : base(defaultCreationParameters)
        {
        }

        protected override SpriteText CreateSpriteText() => new AdaptiveSpriteText();

        public ITextPart AddArbitraryDrawable(Drawable drawable) => AddPart(new TextPartManual(new ArbitraryDrawableWrapper(drawable).Yield()));

        public ITextPart AddIcon(IconUsage icon, Action<SpriteText>? creationParameters = null) => AddText(icon.Icon.ToString(), creationParameters);

        private partial class ArbitraryDrawableWrapper : Container, IHasLineBaseHeight
        {
            private readonly IHasLineBaseHeight? lineBaseHeightSource;

            public float LineBaseHeight => lineBaseHeightSource?.LineBaseHeight ?? DrawHeight;

            public ArbitraryDrawableWrapper(Drawable drawable)
            {
                Child = drawable;
                lineBaseHeightSource = drawable as IHasLineBaseHeight;
                AutoSizeAxes = Axes.Both;
            }
        }
    }
}
