using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net.Http;
using System.Security.Claims;

namespace VehicleServices
{
    struct SpeedAlertInfo
    {
        [JsonProperty("speedLimit")]
        public int SpeedLimit { get; set; }
        [JsonProperty("measuredSpeed")]
        public int MeasuredSpeed { get; set; }
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        public SpeedAlertInfo(int speedLimit, int measuredSpeed, string deviceId)
        {
            SpeedLimit = speedLimit;
            MeasuredSpeed = measuredSpeed;
            DeviceId = deviceId;
        }
    }

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
                string deviceId = doc.GetPropertyValue<string>("deviceId");

                var speedAlertInfo = new SpeedAlertInfo(80, speed, deviceId);

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
        public static async Task<HttpResponseMessage> SetSpeedAlert([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequestMessage req, ILogger log, ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal.Identity.IsAuthenticated)
            {
                var claims = claimsPrincipal.Claims;
                foreach (Claim claim in claims)
                {
                    var type = claim.Type;
                    var value = claim.Value;
                }
                return req.CreateResponse(200);
            }
          
            return await Task.FromResult(req.CreateResponse(401));
        }
    }
}
