using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System.Security.Claims;

namespace VehicleServices
{
    public static class NegotiateSignalRFunction
    {
        [FunctionName("negotiate")]
        public static SignalRConnectionInfo GetSignalRInfo(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req, [SignalRConnectionInfo(HubName = "telematicshub")] SignalRConnectionInfo info,
            ILogger log)
        {

            return info;
        }

        [FunctionName("addToGroup")]
        public static Task AddToGroup(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")]HttpRequest req,
        ClaimsPrincipal claimsPrincipal,
        [SignalR(HubName = "chat")] IAsyncCollector<SignalRGroupAction> signalRGroupActions)
        {
            var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
            var userId = userIdClaim.Value;
            var groupName = "myGroup";
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
