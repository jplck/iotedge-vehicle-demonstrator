using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System.Threading.Tasks;

namespace VehicleServices
{

    struct Location
    {
        [JsonProperty("latitude")]
        public double Latitude { get; }
        [JsonProperty("longitude")]
        public double Longitude { get; }
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        public Location(double latitude, double longitude, string deviceId)
        {
            Latitude = latitude;
            Longitude = longitude;
            DeviceId = deviceId;
        }
    }

    public static class VehicleLocation
    {
        [FunctionName("VehicleLocation")]
        public static async Task Run([SignalR(HubName = "telematicshub")] IAsyncCollector<SignalRMessage> signalRMessages, [CosmosDBTrigger(
            databaseName: "vehicleshadow",
            collectionName: "locations",
            ConnectionStringSetting = "ShadowConnection",
            CreateLeaseCollectionIfNotExists = true,
            LeaseCollectionPrefix = "location")]IReadOnlyList<Document> input, ILogger log)
        {
            if (input != null && input.Count > 0)
            {

                foreach (Document doc in input)
                {
                    string deviceId = doc.GetPropertyValue<string>("deviceId");
                    double lat = doc.GetPropertyValue<double>("latitude");
                    double lon = doc.GetPropertyValue<double>("longitude");

                    Location loc = new Location(lat, lon, deviceId);

                    await signalRMessages.AddAsync(new SignalRMessage()
                    {
                        Target = "vehicleLocation",
                        Arguments = new object[] { JsonConvert.SerializeObject(loc) }
                    });

                    log.LogInformation($"Read and send location. Latitude: {lat}, Longitude: {lon}");
                }
            }
        }
    }
}
