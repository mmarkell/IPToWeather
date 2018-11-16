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
      string IP = "107.77.211.143";
      if (req.Query.ContainsKey("IP"))
      {
        IP = req.Query["IP"];
      }
      log.LogInformation($"getting IP {IP}");
      var long_and_lat = await GetLongitudeAndLatitude(IP, log);
      string latitude = long_and_lat["latitude"];
      string longitude = long_and_lat["longitude"];
      string weather = await GetWeather(latitude, longitude);
      log.LogInformation($"Weather: {weather}, longitude: {longitude}, latitude: {latitude}");
      return (ActionResult)new OkObjectResult($"Weather: {weather}, longitude: {longitude}, latitude: {latitude}");
    }
    public static async Task<Dictionary<string, string>> GetLongitudeAndLatitude(string IP, ILogger log)
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
      return new Dictionary<string, string>
      {
        {"longitude", longAndLat.longitude },
        {"latitude", longAndLat.latitude }
      };
    }

    public static async Task<string> GetWeather(string latitude, string longitude)
    {
      string url = $"http://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}139&appid=0068e0a7d4f9bdd04a3f953df13dea07";
      var values = new Dictionary<string, string>
      {
      };
      var content = new FormUrlEncodedContent(values);
      var response = await client.PostAsync(url, content);
      var res = await response.Content.ReadAsStringAsync();
      return res;
    }

    public class LongAndLat
    {
      public string longitude { get; set; }
      public string latitude { get; set; }
    }
    public class FunctionInput
    {
      public string IP { get; set; }
    }
  }
}
