// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Testing;

namespace YouTubePlayerEX.App.Tests.Visual
{
    public abstract partial class YouTubePlayerEXTestScene : TestScene
    {
        protected override ITestSceneTestRunner CreateRunner() => new YouTubePlayerEXTestSceneTestRunner();

        private partial class YouTubePlayerEXTestSceneTestRunner : YouTubePlayerEXAppBase, ITestSceneTestRunner
        {
            private TestSceneTestRunner.TestRunner runner;

            protected override void LoadAsyncComplete()
            {
                base.LoadAsyncComplete();
                Add(runner = new TestSceneTestRunner.TestRunner());
            }

            public void RunTestBlocking(TestScene test) => runner.RunTestBlocking(test);
        }
    }
}
