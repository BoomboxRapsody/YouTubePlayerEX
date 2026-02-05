// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Graphics;
using YouTubePlayerEX.App.Graphics.Shaders;

namespace YouTubePlayerEX.App.Graphics
{
    public interface IMultiShaderBufferedDrawable : IBufferedDrawable
    {
        IReadOnlyList<ICustomizedShader> Shaders { get; }
    }
}
