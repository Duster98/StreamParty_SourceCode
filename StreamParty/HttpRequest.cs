using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

namespace FilmParty
{
    public class HttpRequest
    {

        public string[] getAllFeeds()
        {
            string[] AllFeeds = new string[10];
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://io.adafruit.com/api/v2/" + MqttConnection.UserName + "/feeds/?X-AIO-Key=" + MqttConnection.AioKey);
                httpWebRequest.Method = "GET";

                string datareceived = "";
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    datareceived = streamReader.ReadToEnd();
                }

                List<FeedsJson.FeedProprety> feedsProprety = JsonConvert.DeserializeObject<List<FeedsJson.FeedProprety>>(datareceived);

                for (int i = 0; i < feedsProprety.Count; i++)
                {
                    AllFeeds[i] = feedsProprety[i].key;
                }

                return AllFeeds;
            }
            catch (Exception)
            {
                return AllFeeds;
            }
        }

        public bool createFeed(string name, string description, string Key_EndToEnd, string Url, string control)
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://io.adafruit.com/api/v2/" + MqttConnection.UserName + "/feeds?X-AIO-Key=" + MqttConnection.AioKey + "&group_key=default");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";


                string key = getCorretName(name);
                if (key == "")
                {
                    return false;
                }

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = "{\"description\":\"" + description + "\"," +
                                  "\"key\":\"" + key + "\"," +
                                  "\"license\":\"" + Key_EndToEnd + "*" + Url + "*" + control + "\"," +
                                  "\"name\":\"" + name + "\"}";
                    streamWriter.Write(json);
                }

                string datareceived = "";
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    datareceived = streamReader.ReadToEnd();
                }
                FeedsJson.FeedProprety feedProprety = JsonConvert.DeserializeObject<FeedsJson.FeedProprety>(datareceived);
                MqttConnection.setChatRoomNameKey(feedProprety.key);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateFeed(string Name, string KeyName, string description, string Key_EndToEnd, string Url, string control)
        {
            try
            {
                var httpWebRequest = await Task.Run(() => (HttpWebRequest)WebRequest.Create("https://io.adafruit.com/api/v2/" + MqttConnection.UserName + "/feeds/" + KeyName + "?X-AIO-Key=" + MqttConnection.AioKey));
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "PATCH";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string json = "{\"description\":\"" + description + "\"," +
                                  "\"key\":\"" + KeyName + "\"," +
                                  "\"license\":\"" + Key_EndToEnd + "*" + Url + "*" + control + "\"," +
                                  "\"name\":\"" + Name + "\"}";
                    streamWriter.Write(json);
                }

                string datareceived = "";
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    datareceived = streamReader.ReadToEnd();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteFeed(string KeyName)
        {
            try
            {
                var httpWebRequest = await Task.Run(() => (HttpWebRequest)WebRequest.Create("https://io.adafruit.com/api/v2/" + MqttConnection.UserName + "/feeds/" + KeyName + "?X-AIO-Key=" + MqttConnection.AioKey));
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "DELETE";

                string datareceived = "";
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    datareceived = streamReader.ReadToEnd();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<string> getJsonData(string ReqUrl)
        {

            var httpWebRequest = await Task.Run(() => (HttpWebRequest)WebRequest.Create(ReqUrl));
            httpWebRequest.Method = "GET";
            httpWebRequest.ContentType = "application/json";

            string datareceived = "";
            var httpResponse = await Task.Run(() => HTTP_Request(httpWebRequest));
            if (httpResponse != null)
            {
                using (var streamReader = await Task.Run(() => new StreamReader(httpResponse.GetResponseStream())))
                {
                    datareceived = streamReader.ReadToEnd();
                }
                return datareceived;
            }
            else
            {
                return "";
            }
        }

        internal HttpWebResponse HTTP_Request(HttpWebRequest httpWebRequest)
        {
            try
            {
                return (HttpWebResponse)httpWebRequest.GetResponse();
            }
            catch (WebException)
            {
                httpWebRequest.Abort();
                return null;
            }
            catch (Exception)
            {
                httpWebRequest.Abort();
                return null;
            }
        }

        public string getCorretName(string name)
        {
            name = name.Trim();
            string key = name.ToLower();
            if (key.Contains(" "))
            {
                key = key.Replace(' ', '-');
            }
            return key;
        }
    }
}
