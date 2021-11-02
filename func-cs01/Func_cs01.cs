using System.IO;
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

namespace Func_cs01
{
    public static class FuncCs01
    {
        private static readonly string TargetUrl = "";
        private static readonly string AudienceId = "";
        private static readonly HttpClient HttpClient;
        static FuncCs01()
        {
            HttpClient = new HttpClient();
        }

        [FunctionName("call-from-cs")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request [call-from-cs].");
            string funcType = req.Query["type"].ToString();
            System.Console.WriteLine($"funcType {funcType}");
            if (string.IsNullOrWhiteSpace(funcType))
            {
                log.LogError("Query Parameter is not found");
                return new ForbidResult("Specify API type to be called.");
            }
            log.LogInformation($"Query Parameter: /api/call-from-cs?{funcType}");
            switch (funcType.ToLower())
            {
                case "c02":
                case "j02":
                    ResMsg resMsg = new ResMsg();
                    try
                    {
                        string accessToken = await new AzureServiceTokenProvider().GetAccessTokenAsync(AudienceId);
                        HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
                        HttpResponseMessage ResponseMessage = HttpClient.GetAsync(TargetUrl).Result;
                        string ResponseString = await ResponseMessage.Content.ReadAsStringAsync();
                        resMsg.status = ResponseMessage.StatusCode;
                        resMsg.message = ResponseString;
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

                default:
                    return new BadRequestObjectResult($"Your specified API type is out of scope[{funcType}].");
            }
        }
    }
}
