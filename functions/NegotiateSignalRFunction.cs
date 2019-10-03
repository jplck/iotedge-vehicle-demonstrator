using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;

namespace VehicleServices
{
    public static class NegotiateSignalRFunction
    {
        [FunctionName("negotiate")]
        public static IActionResult Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req, [SignalRConnectionInfo(HubName = "telematicshub")] SignalRConnectionInfo info,
            ILogger log)
        {
            
            return info != null
                ? (ActionResult)new OkObjectResult(info)
                : new NotFoundObjectResult("Failed to load SignalR info.");
        }
    }
}
