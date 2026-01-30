// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using YouTubePlayerEX.App.Graphics.UserInterface;

namespace YouTubePlayerEX.App.Tests.Visual.UserInterface
{
    [TestFixture]
    public partial class TestSceneVideoMetadataDisplay : YouTubePlayerEXTestScene
    {
        // Add visual tests to ensure correct behaviour of your game: https://github.com/ppy/osu-framework/wiki/Development-and-Testing
        // You can make changes to classes associated with the tests and they will recompile and update immediately.

        private VideoMetadataDisplay videoMetadataDisplay;

        public TestSceneVideoMetadataDisplay()
        {
            Add(videoMetadataDisplay = new VideoMetadataDisplay() { Width = 400, Height = 60 });

            AddStep("change metadata", () => videoMetadataDisplay.UpdateVideo("uWMr16O_Aso"));
        }
    }
}
