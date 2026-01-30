// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using YouTubePlayerEX.App.Graphics.UserInterface;
using YouTubePlayerEX.App.Online;

namespace YouTubePlayerEX.App.Tests.Visual.UserInterface
{
    [TestFixture]
    public partial class TestSceneCommentDisplay : YouTubePlayerEXTestScene
    {
        // Add visual tests to ensure correct behaviour of your game: https://github.com/ppy/osu-framework/wiki/Development-and-Testing
        // You can make changes to classes associated with the tests and they will recompile and update immediately.

        private CommentDisplay videoMetadataDisplay;

        [Resolved]
        private YouTubeAPI api { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(videoMetadataDisplay = new CommentDisplay(api.GetComment("UgyMERjWGh230ezjtuN4AaABAg")) { Width = 600 });
        }
    }
}
