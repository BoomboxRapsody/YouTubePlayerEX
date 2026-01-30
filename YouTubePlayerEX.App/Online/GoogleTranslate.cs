// Copyright (c) 2026 BoomboxRapsody <boomboxrapsody@gmail.com>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using osu.Framework.Configuration;
using osu.Framework.Extensions;
using osu.Framework.Logging;
using YouTubePlayerEX.App.Localisation;

namespace YouTubePlayerEX.App.Online
{
    public class GoogleTranslate
    {
        private FrameworkConfigManager frameworkConfig;
        private YouTubePlayerEXAppBase app;
        public GoogleTranslate(YouTubePlayerEXAppBase app, FrameworkConfigManager frameworkConfig)
        {
            this.frameworkConfig = frameworkConfig;
            this.app = app;
        }

        public string Translate(string text, GoogleTranslateLanguage translateLanguageFrom = GoogleTranslateLanguage.auto)
        {
            string responseText = string.Empty;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://translate.googleapis.com/translate_a/single?client=gtx&sl=" + translateLanguageFrom.ToString() + "&tl=" + app.CurrentLanguage.Value + "&dt=t&q=" + HttpUtility.HtmlEncode(text));
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

            string finalResult = ParseTranslatedValue(responseText);

            return finalResult;
        }

        private string ParseTranslatedValue(string jsonString)
        {
            // Get all json data
            var jsonData = JsonConvert.DeserializeObject<List<dynamic>>(jsonString);

            // Extract just the first array element (This is the only data we are interested in)
            var translationItems = jsonData[0];

            // Translation Data
            string translation = "";

            // Loop through the collection extracting the translated objects
            foreach (object item in translationItems)
            {
                // Convert the item array to IEnumerable
                IEnumerable translationLineObject = item as IEnumerable;

                // Convert the IEnumerable translationLineObject to a IEnumerator
                IEnumerator translationLineString = translationLineObject.GetEnumerator();

                // Get first object in IEnumerator
                translationLineString.MoveNext();

                // Save its value (translated text)
                translation += string.Format(" {0}", Convert.ToString(translationLineString.Current));
            }

            // Remove first blank character
            if (translation.Length > 1) { translation = translation.Substring(1); }

            return translation;
        }
    }
}
