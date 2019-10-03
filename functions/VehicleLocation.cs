using System;
using System.Collections.Generic;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System.Threading.Tasks;

namespace VehicleServices
{
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

                    Location loc = new Location(lat, lon);

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
