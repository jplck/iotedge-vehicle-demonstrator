using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
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
        private ModuleClient _client;

        private HubConnection Hub
        {
            get
            {
                return _hub;
            }
            set
            {
                _ = value ?? throw new ArgumentNullException("hub", "Hub connection cannot be null");
                _hub = value;
            }
        }

        private ModuleClient Client
        {
            get
            {
                return Hub.GetClient();
            }
        }

        private ITwinUpdateReceiver propertyReceiver;

        private TwinCollection reportedProperties;

        private bool reportedPropertiesOutOfSync = false;

        public ModuleTwin(ITwinUpdateReceiver receiver)
        {
            _ = receiver ?? throw new ArgumentNullException("receiver", "Receiver cannot be null");
            _hub = HubConnection.Instance;
            propertyReceiver = receiver;
        }

        public async Task Init()
        {
            await Client.SetDesiredPropertyUpdateCallbackAsync(DesiredPropertiesCallback, null);
            await LoadTwin();
        }

        private async Task LoadTwin()
        {
            var twin = await Client.GetTwinAsync();
            await DesiredPropertiesCallback(twin.Properties.Desired, null);
        }

        private async Task DesiredPropertiesCallback(TwinCollection desiredProperties, object userContext)
        {
            await propertyReceiver.UpdatedDesiredPropertiesReceivedAsync(desiredProperties);

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
            await Client.UpdateReportedPropertiesAsync(reportedProperties);
            reportedPropertiesOutOfSync = false;
        }

        public void AddReportedProperties(TwinCollection properties)
        {
            reportedPropertiesOutOfSync = true;
            reportedProperties = properties;
        }
    }
}
