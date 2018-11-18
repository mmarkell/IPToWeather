using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace mm116.Function
{
    public static class HttpTriggerIPToWeather
    {
        private static readonly HttpClient client = new HttpClient();

        [FunctionName("HttpTriggerIPToWeather")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            string IP = req.Query.ContainsKey("IP") ? req.Query["IP"].ToString() : "107.77.211.143";
            log.LogInformation($"getting IP {IP}");
            LongAndLat long_and_lat = await GetLongitudeAndLatitude(IP, log);
            string latitude = long_and_lat.latitude;
            string longitude = long_and_lat.longitude;
            WeatherResponse weather = await GetWeather(latitude, longitude);
            var weather_string = JsonConvert.SerializeObject(weather);
            log.LogInformation(weather_string);
            return (ActionResult)new OkObjectResult(weather_string);
        }
        public static async Task<LongAndLat> GetLongitudeAndLatitude(string IP, ILogger log)
        {
            IP = IP + "?access_key=8c679b0f90031df872029b7eda9a590c";
            string url = "http://api.ipstack.com/" + IP;
            var values = new Dictionary<string, string>
            {
            };
            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync(url, content);
            var res = await response.Content.ReadAsStringAsync();
            var longAndLat = JsonConvert.DeserializeObject<LongAndLat>(res);
            return longAndLat;
        }

        public static async Task<WeatherResponse> GetWeather(string latitude, string longitude)
        {
            string url = $"http://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}139&appid=0068e0a7d4f9bdd04a3f953df13dea07";
            var values = new Dictionary<string, string>
            {
            };
            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync(url, content);
            var res = await response.Content.ReadAsStringAsync();

            var weather = JsonConvert.DeserializeObject<WeatherResponse>(res);
            return weather;
        }

        public class LongAndLat
        {
            public string longitude { get; set; }
            public string latitude { get; set; }
        }
        public class WeatherResponse
        {
            public DescriptionParam[] Weather { get; set; }
            public string[] Errors { get; set; }
            public MainParam Main { get; set; }
            public SysParam Sys { get; set; }
        }

        public class DescriptionParam
        {
            public string description { get; set; }
        }

        public class MainParam
        {
            public string temp { get; set; }
            public string temp_min { get; set; }
            public string temp_max { get; set; }
        }

        public class SysParam
        {
            public string sunrise { get; set; }
            public string sunset { get; set; }
        }
    }
}
