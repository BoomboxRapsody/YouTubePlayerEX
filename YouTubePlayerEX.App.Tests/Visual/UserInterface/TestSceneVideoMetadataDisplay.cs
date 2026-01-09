using NUnit.Framework;
using YouTubePlayerEX.App.Graphics.UserInterface;

namespace YouTubePlayerEX.App.Tests.Visual.UserInterface
{
    [TestFixture]
    public partial class TestSceneVideoMetadataDisplay : YouTubePlayerEXTestScene
    {
        // Add visual tests to ensure correct behaviour of your game: https://github.com/ppy/osu-framework/wiki/Development-and-Testing
        // You can make changes to classes associated with the tests and they will recompile and update immediately.

        public TestSceneVideoMetadataDisplay()
        {
            Add(new VideoMetadataDisplay() { Width = 400, Height = 60 });
        }
    }
}
