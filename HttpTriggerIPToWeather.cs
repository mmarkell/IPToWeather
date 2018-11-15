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

      string IP = req.Query["IP"];

      string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
      dynamic data = JsonConvert.DeserializeObject(requestBody);
      IP = IP ?? data?.IP;

      log.LogInformation("getting IP");
      IP = await GetLongitudeAndLatitude(IP);

      return IP != null
          ? (ActionResult)new OkObjectResult($"Hello, {IP}")
          : new BadRequestObjectResult("Please pass an IP in the request body");
    }
    public static async Task<string> GetLongitudeAndLatitude(string IP)
    {
      IP = "107.77.211.143" + "?access_key=8c679b0f90031df872029b7eda9a590c";
      string url = "http://api.ipstack.com/" + IP;
      var values = new Dictionary<string, string>
      {
      };
      var content = new FormUrlEncodedContent(values);
      var response = await client.PostAsync(url, content);
      var responseString = await response.Content.ReadAsStringAsync();
      return responseString;
    }
  }
}
