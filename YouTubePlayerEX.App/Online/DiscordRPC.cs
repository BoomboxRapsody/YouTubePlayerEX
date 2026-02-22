// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using DiscordRPC;
using DiscordRPC.Events;
using DiscordRPC.Message;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;

namespace YouTubePlayerEX.App.Online
{
    public partial class DiscordRPC : CompositeDrawable
    {
        public const string DISCORD_APP_ID = "1474920449442840586";
        public static DiscordRpcClient client;

        private IDiscordRPCEvents? events;

        [BackgroundDependencyLoader]
        private void load()
        {
            // Create the client and setup some basic events
            client = new DiscordRpcClient(DISCORD_APP_ID, -1);

            // Register the URI scheme. 
            // This is how Discord will launch your game when a user clicks on a join or spectate button.
            client.RegisterUriScheme();

            // Listen to some events
            client.Subscribe(EventType.Join | EventType.JoinRequest);   // Tell Unity we want to handle these events ourselves.
            client.OnJoinRequested += OnJoinRequested;                  // Another Discord user has requested to join our game
            client.OnJoin += OnJoin;                                    // Our Discord client wants to join a specific lobby

            client.OnReady += (sender, e) =>
            {
                Logger.Log($"[Discord] Connected to discord with user {e.User.Username}", LoggingTarget.Runtime);
                Logger.Log($"[Discord] Avatar: {e.User.GetAvatarURL(User.AvatarFormat.WebP)}", LoggingTarget.Runtime);
                Logger.Log($"[Discord] Decoration: {e.User.GetAvatarDecorationURL()}", LoggingTarget.Runtime);
            };

            //Connect to the RPC
            client.Initialize();
            Logger.Log("[Discord] Discord Rich Presence system initialized.", LoggingTarget.Runtime);
        }

        public void RegisterEvents(IDiscordRPCEvents events)
        {
            this.events = events;
        }

        public void UpdatePresence(RichPresence richPresence) => client.SetPresence(richPresence);

        public void ClearPresence() => client.ClearPresence();

        private void OnJoin(object sender, JoinMessage args)
        {
            events?.OnJoin.Invoke(sender, args);
        }

        private void OnJoinRequested(object sender, JoinRequestMessage args)
        {
            events?.OnJoinRequested.Invoke(sender, args);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            client.OnJoinRequested -= OnJoinRequested;
            client.OnJoin -= OnJoin;

            client.Dispose();
        }

        public class DiscordRPCEvents
        {
            public OnJoinRequestedEvent OnJoinRequested;

            public OnJoinEvent OnJoin;
        }
    }
}
