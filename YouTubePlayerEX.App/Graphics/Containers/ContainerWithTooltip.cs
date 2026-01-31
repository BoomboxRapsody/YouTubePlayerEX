// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Localisation;

namespace YouTubePlayerEX.App.Graphics.Containers
{
    public partial class ContainerWithTooltip : Container, IHasTooltip
    {
        public LocalisableString TooltipText { get; set; }
    }
}
