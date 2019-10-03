using System.Threading.Tasks;
using Microsoft.Azure.Devices.Shared;

namespace VehicleDemonstrator.Shared.Connectivity
{
    public abstract class ModuleTwin
    {
        private HubConnection _hub;

        public ModuleTwin()
        {
            _hub = HubConnection.Instance;
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
            UpdatedDesiredPropertiesReceived(desiredProperties);

            /*Update reported properties at the end to include changed made in the delegate 
              method UpdatedDesiredPropertiesReceived as well.
            */
            await SendReportedProperties();
        }

        public async Task SendReportedProperties()
        {
            await _hub.GetClient().UpdateReportedPropertiesAsync(ProvideReportedProperties());
        }

        public abstract void UpdatedDesiredPropertiesReceived(TwinCollection desiredProperties);
        public abstract TwinCollection ProvideReportedProperties();
    }
}
