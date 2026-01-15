using System.Runtime.InteropServices;
using osu.Framework.Graphics.Shaders.Types;

namespace YouTubePlayerEX.App.Graphics.UserInterfaceV2
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public record struct TriangleBorderData
    {
        public UniformFloat Thickness;
        public UniformFloat TexelSize;
        private readonly UniformPadding8 pad1;
    }
}
