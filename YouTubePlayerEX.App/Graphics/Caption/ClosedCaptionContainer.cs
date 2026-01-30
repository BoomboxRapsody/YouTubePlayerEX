// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;
using YoutubeExplode.Videos.ClosedCaptions;
using YouTubePlayerEX.App.Config;
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
        private ClosedCaptionLanguage captionLanguage;
        private Container captionContainer;

        public ClosedCaptionContainer(YouTubeVideoPlayer videoPlayer, ClosedCaptionTrack captionTrack, ClosedCaptionLanguage captionLanguage)
        {
            this.videoPlayer = videoPlayer;
            this.captionTrack = captionTrack;
            this.captionLanguage = captionLanguage;
            Padding = new MarginPadding(32);
            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
            AlwaysPresent = true;
        }

        public void UpdateCaptionTrack(ClosedCaptionLanguage captionLanguage, ClosedCaptionTrack captionTrack)
        {
            this.captionLanguage = captionLanguage;
            if (captionTrack != null)
                this.captionTrack = captionTrack;
            else
                this.captionTrack = null;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(captionContainer = new Container
            {
                AutoSizeAxes = Axes.Both,
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                AutoSizeDuration = 350,
                AutoSizeEasing = Easing.OutQuart,
                Masking = true,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                        Alpha = 0.5f
                    },
                    spriteText = new AdaptiveSpriteText(false)
                    {
                        Font = YouTubePlayerEXApp.DefaultFont.With(family: "NotoSansKR", size: 24),
                        Margin = new MarginPadding(4),
                    }
                }
            });
        }

        public void UpdateControlsVisibleState(bool state)
        {
            captionContainer.Margin = new MarginPadding
            {
                Bottom = state ? 90 : 0
            };
        }

        protected override void Update()
        {
            base.Update();

            if (captionTrack == null)
                Hide();
            else
                Show();

            if (captionTrack != null)
            {
                try
                {
                    var caption = captionTrack.TryGetByTime(TimeSpan.FromSeconds(videoPlayer.VideoProgress.Value));
                    if (caption != null)
                    {
                        var text = caption.Text; // "collection acts as the parent collection"
                        spriteText.Text = text;
                        captionContainer.FadeIn(150, Easing.OutQuart);
                    }
                    else
                    {
                        captionContainer.FadeOut(150, Easing.OutQuart);
                    }
                }
                catch
                {
                    captionContainer.FadeOut(150, Easing.OutQuart);
                }
            }
        }
    }
}
