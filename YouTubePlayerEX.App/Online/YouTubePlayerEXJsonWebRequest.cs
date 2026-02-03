// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Network;

namespace YouTubePlayerEX.App.Online
{
    public class YouTubePlayerEXJsonWebRequest<T> : JsonWebRequest<T>
    {
        public YouTubePlayerEXJsonWebRequest(string uri)
            : base(uri)
        {
        }

        public YouTubePlayerEXJsonWebRequest()
        {
        }

        protected override string UserAgent => "YouTube-Player-EX";
    }
}
