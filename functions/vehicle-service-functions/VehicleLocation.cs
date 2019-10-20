using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.EventHubs;
using VehicleDemonstrator.Shared.Telemetry;
using System.Text;

namespace VehicleServices
{

    struct TripData
    {
        [JsonProperty("latitude")]
        public double Latitude { get; }
        [JsonProperty("longitude")]
        public double Longitude { get; }
        [JsonProperty("tripDistance")]
        public double TripDistance { get; }
        [JsonProperty("tripTime")]
        public double TripTime { get; }
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        public TripData(TelemetryContainer<Trip> tripContainer)
        {
            var payload = tripContainer.GetPayload();
            var coord = payload.GetCoordinates();
            Latitude = coord.GetLatitude();
            Longitude = coord.GetLongitude();
            DeviceId = tripContainer.GetDeviceId();
            TripDistance = payload.GetTripDistance();
            TripTime = payload.GetTripTime();
        }
    }

    public static class VehicleLocation
    {
        [FunctionName("VehicleLocation")]
        public static async Task Run(
            [SignalR(HubName = "telematicshub")] IAsyncCollector<SignalRMessage> signalRMessages,
            [EventHubTrigger("iotedge-trip-event-hub", Connection = "TripHubConnectionReader")] EventData[] tripEventData,
            ILogger log)
        {

            foreach (EventData eventData in tripEventData)
            {
                string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                TelemetryContainer<Trip> container = TelemetrySegmentFactory<TelemetryContainer<Trip>>.FromJsonString(messageBody);

                TripData tripData = new TripData(container);

                await signalRMessages.AddAsync(new SignalRMessage()
                {
                    Target = "vehicleLocation",
                    Arguments = new object[] { JsonConvert.SerializeObject(tripData) }
                });

                log.LogInformation($"Read and send location. Latitude: {tripData.Latitude}, Longitude: {tripData.Longitude}");
            }

        }
    }
}
