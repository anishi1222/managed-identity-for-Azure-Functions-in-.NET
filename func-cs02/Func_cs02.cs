using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Func_cs02
{
    public static class FuncCs02
    {
        [FunctionName("auth-cs")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            var data = await new StreamReader(req.Body).ReadToEndAsync();
            req.Headers.ToList().ForEach(x => log.LogInformation($"{x.Key.ToString()}: {x.Value.ToString()}"));
            return new OkObjectResult("This is responded by function [auth-cs] in func-cs02. It means HTTP triggered function executed successfully.");
        }
    }
}
