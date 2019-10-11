using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

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
        public long TripTime { get; }
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        public TripData(double latitude, double longitude, string deviceId, double tripDistance, long tripTime)
        {
            Latitude = latitude;
            Longitude = longitude;
            DeviceId = deviceId;
            TripDistance = tripDistance;
            TripTime = tripTime;
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
                    var coords = doc.GetPropertyValue<JObject>("coordinates");
                    double lat = coords.Value<double>("latitude");
                    double lon = coords.Value<double>("longitude"); ;
                    double tripDistance = coords.Value<double>("tripDistance"); ;
                    long tripTime = coords.Value<long>("tripTime"); ;

                    TripData loc = new TripData(lat, lon, deviceId, tripDistance, tripTime);

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
