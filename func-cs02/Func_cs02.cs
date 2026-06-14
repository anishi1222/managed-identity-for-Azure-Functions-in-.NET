using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Func_cs02
{
    public class FuncCs02
    {
        private readonly ILogger<FuncCs02> logger;

        public FuncCs02(ILogger<FuncCs02> logger)
        {
            this.logger = logger;
        }

        [Function("auth-cs")]
        public IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
        {
            foreach (var header in req.Headers)
            {
                logger.LogInformation("{HeaderName}: {HeaderValue}", header.Key, header.Value.ToString());
            }

            return new OkObjectResult("This is responded by function [auth-cs] in func-cs02. It means HTTP triggered function executed successfully.");
        }
    }
}
