using System.Threading.Tasks;
using Microsoft.Azure.Devices.Shared;

namespace VehicleDemonstrator.Shared.Connectivity
{

    public interface ITwinUpdateReceiver
    {
        Task UpdatedDesiredPropertiesReceivedAsync(TwinCollection desiredProperties);
    }

    public class ModuleTwin
    {
        private HubConnection _hub;

        private ITwinUpdateReceiver propertyReceiver;

        private TwinCollection reportedProperties;

        private bool reportedPropertiesOutOfSync = false;

        public ModuleTwin(ITwinUpdateReceiver receiver = null)
        {
            _hub = HubConnection.Instance;
            propertyReceiver = receiver;
        }

        public async Task Init()
        {
            await _hub.GetClient().SetDesiredPropertyUpdateCallbackAsync(DesiredPropertiesCallback, null);
            await LoadTwin();
        }

        private async Task LoadTwin()
        {
            var twin = await _hub.GetClient().GetTwinAsync();
            await DesiredPropertiesCallback(twin.Properties.Desired, null);
        }

        private async Task DesiredPropertiesCallback(TwinCollection desiredProperties, object userContext)
        {
            if (propertyReceiver != null)
            {
                await propertyReceiver.UpdatedDesiredPropertiesReceivedAsync(desiredProperties);
            }

            /*Update reported properties at the end to include changed made in the delegate 
              method UpdatedDesiredPropertiesReceived as well.
            */

            await SendReportedProperties();
            
        }

        public async Task SendReportedProperties()
        {
            if (!reportedPropertiesOutOfSync)
            {
                return;
            }
            await _hub.GetClient().UpdateReportedPropertiesAsync(reportedProperties);
            reportedPropertiesOutOfSync = false;
        }

        public void AddReportedProperties(TwinCollection properties)
        {
            reportedPropertiesOutOfSync = true;
            reportedProperties = properties;
        }
    }
}
