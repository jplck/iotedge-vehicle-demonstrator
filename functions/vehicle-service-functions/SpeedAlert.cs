using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using System.Threading;
using System.Net.Http;
using System.Net;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace VehicleServices
{
    public static class SpeedAlert
    {
        [FunctionName("SpeedAlert")]
        public static async Task Run([CosmosDBTrigger(
            databaseName: "vehicleshadow",
            collectionName: "odometry",
            ConnectionStringSetting = "ShadowConnection",
            CreateLeaseCollectionIfNotExists = true,
            LeaseCollectionPrefix = "speedAlert")]IReadOnlyList<Document> input,
            [SignalR(HubName = "telematicshub")] IAsyncCollector<SignalRMessage> signalRMessages, ILogger log)
        {
            foreach (Document doc in input)
            {
            
                int speed = doc.GetPropertyValue<int>("speed");

                var speedAlertInfo = new SpeedAlertInfo();
                speedAlertInfo.SpeedLimit = 80;
                speedAlertInfo.MeasuredSpeed = speed;

                if (speed > 80)
                {
                    log.LogInformation($"Exeeded speed limit by: {speed - 80}");
                    await signalRMessages.AddAsync(new SignalRMessage()
                    {
                        Target= "speedAlerts",
                        Arguments=new object[] { JsonConvert.SerializeObject(speedAlertInfo) }
                    });
                }

                await signalRMessages.AddAsync(new SignalRMessage()
                {
                    Target = "odometry",
                    Arguments = new object[] { JsonConvert.SerializeObject(speedAlertInfo) }
                });

                await Task.Yield();

            }
        }

        [FunctionName("SetSpeedAlert")]
        public static IActionResult SetSpeedAlert([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            return (ActionResult)new OkObjectResult(200);
        }
    }
}
