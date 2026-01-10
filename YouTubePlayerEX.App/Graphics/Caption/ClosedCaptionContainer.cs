using System;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;
using YoutubeExplode.Videos.ClosedCaptions;
using YouTubePlayerEX.App.Graphics.Sprites;
using YouTubePlayerEX.App.Graphics.Videos;

namespace YouTubePlayerEX.App.Graphics.Caption
{
    public partial class ClosedCaptionContainer : Container
    {
        public Bindable<bool> UIVisiblity = new Bindable<bool>();

        private AdaptiveSpriteText spriteText;
        private YouTubeVideoPlayer videoPlayer;
        private ClosedCaptionTrack captionTrack;

        public ClosedCaptionContainer(YouTubeVideoPlayer videoPlayer, ClosedCaptionTrack captionTrack)
        {
            this.videoPlayer = videoPlayer;
            this.captionTrack = captionTrack;
            Padding = new MarginPadding(32);
            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
            AlwaysPresent = true;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(new Container
            {
                AutoSizeAxes = Axes.Both,
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                        Alpha = 0.5f
                    },
                    spriteText = new AdaptiveSpriteText
                    {
                        Font = YouTubePlayerEXApp.DefaultFont.With(size: 24),
                    }
                }
            });
        }

        protected override void Update()
        {
            base.Update();
            if (captionTrack != null)
            {
                try
                {
                    var caption = captionTrack.TryGetByTime(TimeSpan.FromSeconds(videoPlayer.VideoProgress.Value));
                    if (caption != null)
                    {
                        var text = caption.Text; // "collection acts as the parent collection"
                        spriteText.Text = text;
                        Show();
                    }
                    else
                    {
                        Hide();
                    }
                }
                catch
                {
                    Hide();
                }
            }
        }
    }
}
