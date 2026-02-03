// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Oauth2.v2;
using Google.Apis.Oauth2.v2.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using YouTubePlayerEX.App.Config;

namespace YouTubePlayerEX.App.Online
{
    public partial class GoogleOAuth2
    {
        private static bool isTestClient_static;

        private YTPlayerEXConfigManager appConfig;

        private bool isTestClient
        {
            get => isTestClient_static;
            set => isTestClient_static = value;
        }

        public GoogleOAuth2(YTPlayerEXConfigManager appConfig, bool isTestClient)
        {
            this.appConfig = appConfig;
            this.isTestClient = isTestClient;
        }

        public BindableBool SignedIn { get; private set; } = new BindableBool();

        private UserCredential credential;

        public async Task<Userinfo> SignIn()
        {
            credential = await getUserCredentialAsync();

            // OAuth2 API 클라이언트를 생성합니다.
            var oauth2Service = new Oauth2Service(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential
            });

            // 사용자의 정보를 가져옵니다.
            Userinfo userInfo = await oauth2Service.Userinfo.Get().ExecuteAsync();

            Logger.Log("signed in to google");

            SignedIn.Value = true;
            appConfig.SetValue<bool>(YTPlayerEXSetting.FinalLoginState, true);

            return userInfo;
        }

        public async Task SignOut()
        {
            if (credential != null)
            {
                await credential.RevokeTokenAsync(CancellationToken.None);

                Logger.Log("signed out to google");

                SignedIn.Value = false;
                appConfig.SetValue<bool>(YTPlayerEXSetting.FinalLoginState, false);
            }
        }

        public string GetAccessToken()
        {
            if (!SignedIn.Value)
                return string.Empty;

            return credential?.Token?.AccessToken;
        }

        private static async Task<UserCredential> getUserCredentialAsync()
        {
            UserCredential credential;

            using (var stream = new FileStream(isTestClient_static ? @"youtube-player-ex-development.json" : @"youtube-player-ex-production.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    new[] { "https://www.googleapis.com/auth/userinfo.email", "https://www.googleapis.com/auth/youtube.readonly", "https://www.googleapis.com/auth/youtube.force-ssl" },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)
                );
            }

            return credential;
        }
    }
}
