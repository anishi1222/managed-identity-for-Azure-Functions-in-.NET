using System;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading;

namespace Func_cs01
{
    public class FuncCs01
    {
        private static readonly IReadOnlyDictionary<string, TargetInfo> TargetInfos = new Dictionary<string, TargetInfo>(StringComparer.OrdinalIgnoreCase)
        {
            ["c02"] = new TargetInfo("https://func-cs02.azurewebsites.net/api/auth-cs", "appID for func-cs02"),
            ["j02"] = new TargetInfo("https://func-j02.azurewebsites.net/api/auth-java", "appID for func-j02")
        };
        private static readonly TokenCredential Credential = new DefaultAzureCredential();

        private readonly IHttpClientFactory httpClientFactory;
        private readonly ILogger<FuncCs01> logger;

        public FuncCs01(IHttpClientFactory httpClientFactory, ILogger<FuncCs01> logger)
        {
            this.httpClientFactory = httpClientFactory;
            this.logger = logger;
        }

        [Function("call-from-cs")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            logger.LogInformation("C# HTTP trigger function processed a request [call-from-cs].");
            string funcType = req.Query["type"].ToString().ToLowerInvariant();
            System.Console.WriteLine($"funcType {funcType}");

            // Check funcType
            if (string.IsNullOrWhiteSpace(funcType))
            {
                logger.LogError("Query Parameter is not found");
                return new ForbidResult("Specify API type to be called.");
            }
            if (!TargetInfos.TryGetValue(funcType, out TargetInfo target))
            {
                logger.LogError("Query Parameter is out of scope: {FuncType}", funcType);
                return new BadRequestObjectResult($"Your specified API type is out of scope[{funcType}].");
            }
            string safeFuncTypeForLog = funcType.Replace("\r", string.Empty).Replace("\n", string.Empty);
            logger.LogInformation("Query Parameter: /api/call-from-cs?{FuncType}", safeFuncTypeForLog);

            ResMsg resMsg = new ResMsg();
            try
            {
                string accessToken = await GetAccessTokenAsync(target.AudienceId, cancellationToken);
                HttpClient httpClient = httpClientFactory.CreateClient();
                using HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, target.TargetUrl);
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                using HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage, cancellationToken);
                string responseString = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
                resMsg.status = responseMessage.StatusCode;
                resMsg.message = responseString;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Exception while calling target function.");
                resMsg.status = HttpStatusCode.ServiceUnavailable;
                resMsg.message = exception.Message;
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

        private static async Task<string> GetAccessTokenAsync(string audience, CancellationToken cancellationToken)
        {
            AccessToken accessToken = await Credential.GetTokenAsync(new TokenRequestContext(new[] { ToDefaultScope(audience) }), cancellationToken);
            return accessToken.Token;
        }

        private static string ToDefaultScope(string audience)
        {
            string normalizedAudience = audience.Trim();

            if (normalizedAudience.EndsWith("/.default", StringComparison.OrdinalIgnoreCase))
            {
                return normalizedAudience;
            }

            if (Guid.TryParse(normalizedAudience, out _))
            {
                normalizedAudience = $"api://{normalizedAudience}";
            }

            return $"{normalizedAudience.TrimEnd('/')}/.default";
        }
    }
}
