using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using osu.Framework.Extensions;
using osu.Framework.Logging;

namespace YouTubePlayerEX.App.Online
{
    public class ReturnYouTubeDislike
    {
        public static ReturnYouTubeDislikesResponse GetDislikes(string videoId)
        {
            string responseText = string.Empty;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://returnyoutubedislikeapi.com/Votes?videoId={videoId}");
                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0";

                using (HttpWebResponse resp = (HttpWebResponse)request.GetResponse())
                {
                    Encoding encode;
                    if (resp.CharacterSet.ToLower() == "utf-8")
                    {
                        encode = Encoding.UTF8;
                    }
                    else
                    {
                        encode = Encoding.Default;
                    }

                    HttpStatusCode status = resp.StatusCode;
                    // response 매시지 중 StatusCode를 가져온다.

                    Console.WriteLine(status);
                    // 정상이면 "OK"

                    Stream respStream = resp.GetResponseStream();
                    // Response Data 내용은 GetResponseStream 메서드로부터 얻어낸 스트림을 읽어 가져올 수 있음
                    using (StreamReader sr = new StreamReader(respStream, encode))
                    {
                        responseText = sr.ReadToEnd();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, e.GetDescription());
            }

            Logger.Log(responseText);

            responseText = $"[{responseText}]";

            List<ReturnYouTubeDislikesResponse> dislikes = JsonConvert.DeserializeObject<List<ReturnYouTubeDislikesResponse>>(responseText);

            return dislikes[0];
        }
    }

    public class ReturnYouTubeDislikesResponse
    {
        public string Id { get; set; }
        public string DateCreated { get; set; }
        public int Likes { get; set; }
        public int RawDislikes { get; set; }
        public int RawLikes { get; set; }
        public int Dislikes { get; set; }
        public float Rating { get; set; }
        public int ViewCount { get; set; }
        public bool Deleted { get; set; }
    }
}
