// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace NekoPlayer.App.Online
{
    public static class NekoPlayerDescriptionParser
    {
        public static List<YouTubeDescriptionTextToken> Parse(string input)
        {
            var tokens = new List<YouTubeDescriptionTextToken>();
            var regex = new Regex(@"https?://[^\s]+|@\w+");

            int lastIndex = 0;

            foreach (Match match in regex.Matches(input))
            {
                // 일반 텍스트
                if (match.Index > lastIndex)
                {
                    tokens.Add(new YouTubeDescriptionTextToken
                    {
                        Type = YouTubeDescriptionTokenType.Text,
                        Value = input.Substring(lastIndex, match.Index - lastIndex)
                    });
                }

                // URL or Mention
                if (match.Value.StartsWith("@"))
                {
                    tokens.Add(new YouTubeDescriptionTextToken
                    {
                        Type = YouTubeDescriptionTokenType.Mention,
                        Value = match.Value
                    });
                }
                else
                {
                    tokens.Add(new YouTubeDescriptionTextToken
                    {
                        Type = YouTubeDescriptionTokenType.Url,
                        Value = match.Value
                    });
                }

                lastIndex = match.Index + match.Length;
            }

            if (lastIndex < input.Length)
            {
                tokens.Add(new YouTubeDescriptionTextToken
                {
                    Type = YouTubeDescriptionTokenType.Text,
                    Value = input.Substring(lastIndex)
                });
            }

            return tokens;
        }
    }
}
