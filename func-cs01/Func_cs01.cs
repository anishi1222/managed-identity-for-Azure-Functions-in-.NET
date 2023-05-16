using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using Microsoft.Azure.Services.AppAuthentication;
using System;
using System.Collections.Concurrent;
using func_c01;

namespace Func_cs01 {
    public static class FuncCs01
    {
        private static readonly ConcurrentDictionary<string, TargetInfo> keyValuePairs = new ConcurrentDictionary<string, TargetInfo>();
        private static readonly HttpClient HttpClient;

        static FuncCs01()
        {
            HttpClient = new HttpClient();
            keyValuePairs.TryAdd("c02", new TargetInfo("https://func-cs02.azurewebsites.net/api/auth-cs", "appID for func-cs02"));
            keyValuePairs.TryAdd("j02", new TargetInfo("https://func-j02.azurewebsites.net/api/auth-java", "appID for func-j02"));
        }

        [FunctionName("call-from-cs")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request [call-from-cs].");
            string funcType = req.Query["type"].ToString().ToLowerInvariant();
            System.Console.WriteLine($"funcType {funcType}");

            // Check funcType
            if (string.IsNullOrWhiteSpace(funcType))
            {
                log.LogError("Query Parameter is not found");
                return new ForbidResult("Specify API type to be called.");
            }
            if(!keyValuePairs.ContainsKey(funcType)) {
                log.LogError("Query Parameter is out of scope: {0}", funcType);
                return new BadRequestObjectResult($"Your specified API type is out of scope[{funcType}].");
            }
            log.LogInformation($"Query Parameter: /api/call-from-cs?{funcType}");

            ResMsg resMsg = new ResMsg();
            try
            {
                TargetInfo target;
                if(keyValuePairs.TryGetValue(funcType, out target)) {
                    string accessToken = await new AzureServiceTokenProvider().GetAccessTokenAsync(target.audienceID);
                    HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                    HttpResponseMessage ResponseMessage = HttpClient.GetAsync(target.targetUrl).Result;
                    string ResponseString = await ResponseMessage.Content.ReadAsStringAsync();
                    resMsg.status = ResponseMessage.StatusCode;
                    resMsg.message = ResponseString;
                }
            }
            catch (Exception e) {
                log.LogError($"Exception: {e.StackTrace}");
                resMsg.status = HttpStatusCode.ServiceUnavailable;
                resMsg.message=e.Message;
            }

            if (resMsg.status.Equals(HttpStatusCode.OK))
            {
                OkObjectResult okObjectResult = new OkObjectResult(resMsg);
                okObjectResult.ContentTypes.Add(MediaTypeNames.Application.Json);
                return okObjectResult;
            }
            else
            {
                BadRequestObjectResult badRequestObjectResult = new BadRequestObjectResult(resMsg);
                badRequestObjectResult.ContentTypes.Add(MediaTypeNames.Application.Json);
                return badRequestObjectResult;
            }
        }
    }
}
