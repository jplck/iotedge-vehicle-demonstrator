using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Net.Http;
using Microsoft.Azure.Documents.Client;
using System;
using Microsoft.Azure.Documents.Linq;
using System.Collections.Generic;
using System.Linq;

namespace VehicleServices
{
    public struct UserVehicle
    {
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("userId")]
        public string UserId { get; set; }
    }

    public static class UserVehicles
    {
        [FunctionName("AddDeviceIdUserPairing")]
        public static async Task<HttpResponseMessage> AddDeviceIdUserPairing(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestMessage req,
            [CosmosDB(databaseName: "vehicleshadow",
                      collectionName: "vehicleregister",
                      ConnectionStringSetting = "ShadowConnection")] IAsyncCollector<UserVehicle> mapping,
            ILogger log, ClaimsPrincipal claimsPrincipal)
        {
            HttpResponseMessage response = req.CreateResponse(200);
 
            if (claimsPrincipal.Identity.IsAuthenticated)
            {
                var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
            
                if (userIdClaim != null && userIdClaim.Value != null)
                {
                    var userId = userIdClaim.Value;
                    log.LogInformation("Authentication valid. Start deserialization of request");

                    string requestBody = await req.Content.ReadAsStringAsync();

                    if (requestBody.Length == 0)
                    {
                        response.Content = new StringContent("No DeviceId/User pairing provided. Please add a payload to your request.");
                        response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                        return await Task.FromResult(response);
                    }

                    UserVehicle userVehiclePayload = JsonConvert.DeserializeObject<UserVehicle>(requestBody);
                    userVehiclePayload.UserId = userId;

                    await mapping.AddAsync(userVehiclePayload);
                    log.LogInformation("DeviceId/User paring added.");
                }
                else
                {
                    return await Task.FromResult(req.CreateResponse(401));
                }

            }

            response.Content = new StringContent("Pairing created.");
            response.StatusCode = System.Net.HttpStatusCode.Created;
            return await Task.FromResult(response);
        }

        [FunctionName("GetDeviceIdUserPairings")]
        public static async Task<HttpResponseMessage> GetDeviceIdUserPairings(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestMessage req,
            [CosmosDB(databaseName: "vehicleshadow",
                      collectionName: "vehicleregister",
                      ConnectionStringSetting = "ShadowConnection")] DocumentClient client,
            ILogger log, ClaimsPrincipal claimsPrincipal)
        {
            HttpResponseMessage response = req.CreateResponse(200);

            if (claimsPrincipal.Identity.IsAuthenticated)
            {
                var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim != null && userIdClaim.Value != null)
                {
                    var userId = userIdClaim.Value;

                    var options = new FeedOptions { EnableCrossPartitionQuery = true };

                    Uri collectionUri = UriFactory.CreateDocumentCollectionUri("vehicleshadow", "vehicleregister");
                    IDocumentQuery<UserVehicle> query = client.CreateDocumentQuery<UserVehicle>(collectionUri, options)
                        .Where(pairing => pairing.UserId == userId)
                        .AsDocumentQuery();
                   
                    var mappings = new List<UserVehicle>();

                    while (query.HasMoreResults)
                    {
                        mappings.Clear();
                        foreach (UserVehicle userVehiclePairing in await query.ExecuteNextAsync())
                        {
                            var pairing = userVehiclePairing;
                            pairing.UserId = null;
                            mappings.Add(pairing);
                        }
                    }

                    string payload = JsonConvert.SerializeObject(mappings);
                    response.Content = new StringContent(payload);
                    response.StatusCode = System.Net.HttpStatusCode.OK;
                    return await Task.FromResult(response);
                }
                else
                {
                    return await Task.FromResult(req.CreateResponse(401));
                }
            }

            response.StatusCode = System.Net.HttpStatusCode.NoContent;
            return await Task.FromResult(response);
        }
    }
}
