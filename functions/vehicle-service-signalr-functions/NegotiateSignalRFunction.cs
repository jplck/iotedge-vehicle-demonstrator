using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System.Security.Claims;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace VehicleServices
{
    public class NegotiateSignalRFunction
    {
        private TelemetryClient telemetryClient;

        public NegotiateSignalRFunction(TelemetryConfiguration config)
        {
            this.telemetryClient = new TelemetryClient(config);
            this.telemetryClient.Context.Cloud.RoleName = "negotiate";
        }

        [FunctionName("negotiate")]
        public SignalRConnectionInfo GetSignalRInfo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req, [SignalRConnectionInfo(HubName = "telematicshub", UserId = "{headers.x-ms-client-principal-id}")] SignalRConnectionInfo info,
            ILogger log)
        {
            this.telemetryClient.TrackEvent(new EventTelemetry("Successfully negotiated SignalR endpoint."));
            return info;
        }

        [FunctionName("addToGroup")]
        public Task AddToGroup(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")]HttpRequest req,
        ClaimsPrincipal claimsPrincipal,
        [SignalR(HubName = "chat")] IAsyncCollector<SignalRGroupAction> signalRGroupActions,
        ILogger log)
        {
            var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
            var userId = userIdClaim.Value;
            var groupName = "vehicletelemetrydemousers";
            log.LogDebug(userId);

            return signalRGroupActions.AddAsync(
                new SignalRGroupAction
                {
                    UserId = userId,
                    GroupName = groupName,
                    Action = GroupAction.Add
                });
        }
    }
}
