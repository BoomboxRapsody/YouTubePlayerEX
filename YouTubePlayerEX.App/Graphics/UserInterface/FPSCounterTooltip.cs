using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Platform;
using osuTK;

namespace YouTubePlayerEX.App.Graphics.UserInterface
{
    public partial class FPSCounterTooltip : CompositeDrawable, ITooltip
    {
        private AdaptiveTextFlowContainer textFlow = null!;

        [Resolved]
        private GameHost gameHost { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;

            CornerRadius = 15;
            Masking = true;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = Color4Extensions.FromHex(@"111"),
                    Alpha = 1,
                    RelativeSizeAxes = Axes.Both,
                },
                new AdaptiveTextFlowContainer(cp =>
                {
                    cp.Font = YouTubePlayerEXApp.DefaultFont.With(weight: "SemiBold");
                })
                {
                    AutoSizeAxes = Axes.Both,
                    TextAnchor = Anchor.TopRight,
                    Margin = new MarginPadding { Left = 5, Vertical = 10 },
                    Text = string.Join('\n', gameHost.Threads.Select(t => t.Name)),
                    ParagraphSpacing = 0,
                },
                textFlow = new AdaptiveTextFlowContainer(cp =>
                {
                    cp.Font = YouTubePlayerEXApp.DefaultFont.With(fixedWidth: true, weight: "Regular");
                    cp.Spacing = new Vector2(-1);
                })
                {
                    Width = 190,
                    Margin = new MarginPadding { Left = 35, Right = 10, Vertical = 10 },
                    AutoSizeAxes = Axes.Y,
                    TextAnchor = Anchor.TopRight,
                    ParagraphSpacing = 0,
                },
            };
        }

        private int lastUpdate;

        protected override void Update()
        {
            int currentSecond = (int)(Clock.CurrentTime / 100);

            if (currentSecond != lastUpdate)
            {
                lastUpdate = currentSecond;

                textFlow.Clear();

                foreach (var thread in gameHost.Threads)
                {
                    var clock = thread.Clock;

                    string maximum = clock.Throttling
                        ? $"/{(clock.MaximumUpdateHz > 0 && clock.MaximumUpdateHz < 10000 ? clock.MaximumUpdateHz.ToString("0") : "∞"),4}"
                        : string.Empty;

                    textFlow.AddParagraph($"{clock.FramesPerSecond:0}{maximum} fps ({clock.ElapsedFrameTime:0.00} ms)");
                }
            }
        }

        public void SetContent(object content)
        {
        }

        public void Move(Vector2 pos)
        {
            Position = pos;
        }
    }
}
