// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using DiscordRPC;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;

namespace YouTubePlayerEX.App.Online
{
    public partial class DiscordRPC : CompositeDrawable
    {
        public const string DISCORD_APP_ID = "1474920449442840586";
        public static DiscordRpcClient client;

        [BackgroundDependencyLoader]
        private void load()
        {
            // Create the client and setup some basic events
            client = new DiscordRpcClient(DISCORD_APP_ID);

            client.OnReady += (sender, e) =>
            {
                Console.WriteLine("Connected to discord with user {0}", e.User.Username);
                Console.WriteLine("Avatar: {0}", e.User.GetAvatarURL(User.AvatarFormat.WebP));
                Console.WriteLine("Decoration: {0}", e.User.GetAvatarDecorationURL());
            };

            //Connect to the RPC
            client.Initialize();
            Logger.Log("[Discord] Discord Rich Presence system initialized.", LoggingTarget.Runtime);
        }

        public void UpdatePresence(RichPresence richPresence) => client.SetPresence(richPresence);

        public void ClearPresence() => client.ClearPresence();

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            client.Dispose();
        }
    }
}
