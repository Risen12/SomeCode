using System;
using VkNet;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using VkNet.Model;
using VkNet.Enums.Filters;
using VkNet.Utils;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Net.Http;

namespace MINIBOT_VK
{
    class Program
    {
        
        static void Main(string[] args)
        {
            VkApi vk = new VkApi();
            WebClient web = new WebClient() { Encoding = Encoding.UTF8 };
            vk.Authorize(new ApiAuthParams
            {
                ApplicationId = 6921538,
                AccessToken = "fa41a2680f04468824782b81d443d4960a4a0e4ea9623b14fdd3851e54f0c9a6146158824131ea2dc71ba",
                Login = "den2033@bk.ru",
                Password = "game2012",

                Settings = Settings.Messages | Settings.All | Settings.Groups

            });

            #region Reuqest for users
            //var options = new VkParameters() { };
            //options.Add<string>("group_id","memeliaz");
            //options.Add<string>("offset","0");
            //options.Add<string>("count","15");

            //var json = JObject.Parse(vk.Call("groups.getMembers", options).RawJson);

            //string ids = string.Join(",", json["response"]["items"].ToArray().Select(e => e.ToString()));

            //options = new VkParameters() { };

            //options.Add<string>("user_ids",ids);
            //options.Add<string>("fields","photo_100,sex");

            //var result = JObject.Parse(vk.Call("users.get", options).RawJson)["response"];

            //foreach (dynamic item in result)
            //{
            //    string sex1 = "";
            //    if (item.sex == "1") sex1 = "Женщина";
            //    else if (item.sex == "2") sex1 = "Мужчина";
            //    else sex1 = "Пол не указан";
            //    Console.WriteLine($"{item.id}_{item.first_name}_{item.last_name}_{item.photo_100} {sex1}");

            //    string url_photo = item.photo_100;
            //    web.DownloadFile(new Uri(url_photo.Replace("?ava=1", "")), $"photo\\photo_{item.id}_{item.first_name}_{item.last_name}.jpg");
            //}
            #endregion

            var options = new VkParameters() { };

            options.Add<string>("group_id", "49956226"); // parameters for long poll call

            dynamic LongPoll = JObject.Parse(vk.Call("groups.getLongPollServer", options).RawJson); //parameters for get important infromation for messages

            string json = String.Empty;

            string url = String.Empty;

            while (true)
            {
                url = string.Format("{0}?act=a_check&key={1}&ts={2}&wait=25&version=3",
                    LongPoll.response.server.ToString(),
                    LongPoll.response.key.ToString(),
                    json != String.Empty ? JObject.Parse(json)["ts"].ToString() : LongPoll.response.ts.ToString());

                json = web.DownloadString(url);

                var jsonCheck = json.IndexOf(":[]}") > -1 ? "" : $"{json} \n";

                List<string> ids = new List<string>();
                var array_of_messages = JObject.Parse(json)["updates"].ToList();
               

                foreach (var item in array_of_messages)
                {
                    if (item["type"].ToString() == "message_new")
                    {
                        string token = @"654c617adbd37f55d2b8a37ad6390d5b3da92940238610fadca34e8b0a5e3b56684b9efc9360f66103fdf";
                        ids.Add(item["object"]["from_id"].ToString());
                        string message = item["object"]["text"].ToString();
                        var options_for_msg = new VkParameters() { };

                        options_for_msg.Add<string>("v", "5.95");
                        options_for_msg.Add<string>("access_token", token);
                        options_for_msg.Add<string>("user_id",item["object"]["from_id"].ToString());
                        options_for_msg.Add<string>("random_id", "0");
                        options_for_msg.Add<string>("group_id", "49956226");
                        options_for_msg.Add<string>("chat_id",item["object"]["id"].ToString());
                        //if (options_for_msg["user_id"] == "185221407" || options_for_msg["user_id"] == "29874534") options_for_msg.Add<string>("message", "Пошёл нахуй");
                         options_for_msg.Add<string>("message", Weather(message));
                        Console.WriteLine($" {message} ");
                        var result = JObject.Parse(vk.Call("messages.send", options_for_msg).RawJson);
                        Thread.Sleep(200);
                    }
                }
            }


        }
        public static string Weather(string city) // выполнено с помощью OpenWeatherMap https://openweathermap.org/
        {
            string n_city = Translate(city);
            n_city = n_city.Remove(0, 6);
            n_city = n_city.Remove(n_city.IndexOf('"'));
            HttpClient http_helper = new HttpClient();

            try
            {
                string url = $"http://api.openweathermap.org/data/2.5/weather?q={n_city}&units=metric&appid=a06e17b4ef5f8709eea7a3780039ca29";
                string data = http_helper.GetStringAsync(url).Result;
                dynamic rec = JObject.Parse(data);
                return $"Вы выбрали город {city}.\n" +
                    $"Температура в данном городе:{rec.main.temp}°C.\n" +
                    $"Скорость ветра равна {rec.wind.speed} км/ч \n" +
                    $"Облачность равна {rec.clouds.all}%";
            }
            catch
            {
                return "Ошибка запроса";
            }

        }
        public static string Translate(string word) // Выполнено с помощью Яндекс Переводчик https://translate.yandex.ru/
        {
            char[] minus = new char[] {'[',']','\\','r','n'};
            HttpClient httptranslate = new HttpClient();
            if (word == "") return word;
            string key = "trnsl.1.1.20190509T155828Z.b547350f31959f40.500976517bc65a5b353dc17a2d87f000901d314e";
            string url = $"https://translate.yandex.net/api/v1.5/tr.json/translate?key={key}&text={word}&lang=en";
            string data = httptranslate.GetStringAsync(url).Result;
            dynamic trans = JObject.Parse(data)["text"];
            string result = Convert.ToString(trans);
            return result;

        }
        
    }
}
