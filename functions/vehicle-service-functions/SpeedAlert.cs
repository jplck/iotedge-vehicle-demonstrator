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
using Microsoft.Azure.EventHubs;
using System.Text;
using VehicleDemonstrator.Shared.Telemetry;
using VehicleDemonstrator.Shared.Telemetry.Odometry;

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
        public static async Task Run(
            [EventHubTrigger("iotedge-odometry-event-hub", Connection = "OdometerHubConnectionListener")] EventData[] events,
            [SignalR(HubName = "telematicshub")] IAsyncCollector<SignalRMessage> signalRMessages, ILogger log)
        {
            foreach (EventData eventData in events)
            {
                string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                TelemetryContainer<Odometry> container = TelemetrySegmentFactory<TelemetryContainer<Odometry>>.FromJsonString(messageBody);

                int speed = container.GetPayload().GetSpeed();
                string deviceId = container.GetDeviceId();

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
