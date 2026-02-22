// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osuTK.Graphics;
using YouTubePlayerEX.App.Config;
using YouTubePlayerEX.App.Extensions;
using YouTubePlayerEX.App.Localisation;
using YouTubePlayerEX.App.Online;

namespace YouTubePlayerEX.App.Graphics.UserInterface
{
    public partial class DiscordProfileImage : CompositeDrawable, IHasTooltip
    {
        private Sprite profileImage;

        private DiscordRPC.User user;

        private LoadingSpinner loading;

        [Resolved]
        private TextureStore textureStore { get; set; }

        [Resolved]
        private YouTubeAPI api { get; set; }

        public virtual LocalisableString TooltipText { get; protected set; }

        [Resolved]
        private YouTubePlayerEXAppBase app { get; set; }

        private Bindable<VideoMetadataTranslateSource> translationSource = new Bindable<VideoMetadataTranslateSource>();

        public DiscordProfileImage(float size = 30)
        {
            Width = Height = size;
            CornerRadius = size / 2;
            Masking = true;
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black
                },
                profileImage = new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                },
                loading = new LoadingLayer(true, false, false)
            };
        }

        private Sample clickAudio;

        [BackgroundDependencyLoader]
        private void load(ISampleStore tracks)
        {
            usernameDisplayMode = appConfig.GetBindable<UsernameDisplayMode>(YTPlayerEXSetting.UsernameDisplayMode);
            translationSource = appConfig.GetBindable<VideoMetadataTranslateSource>(YTPlayerEXSetting.VideoMetadataTranslateSource);
            clickAudio = tracks.Get("button-select.wav");
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            profileImage.Dispose();
        }

        public void PlayClickAudio()
        {
            clickAudio.Play();
        }

        [Resolved]
        private YTPlayerEXConfigManager appConfig { get; set; }

        protected override bool OnClick(ClickEvent e)
        {
            return base.OnClick(e);
        }

        public void UpdateProfileImage(DiscordRPC.User user)
        {
            Task.Run(async () =>
            {
                this.user = user;
                _ = Task.Run(async () =>
                {
                    await GetProfileImage(user.GetAvatarURL());
                });
                TooltipText = user.Username + $" ({user.ID})";
            });
        }

        private Bindable<UsernameDisplayMode> usernameDisplayMode;

        private CancellationTokenSource profileImageCancellationSource = new CancellationTokenSource();

        public async Task GetProfileImage(string url, CancellationToken cancellationToken = default)
        {
            Schedule(() => loading.Show());
            Texture north = await textureStore.GetAsync(user.GetAvatarURL(), cancellationToken);
            Schedule(() => { profileImage.Texture = north; });
            Schedule(() => loading.Hide());
        }
    }
}
