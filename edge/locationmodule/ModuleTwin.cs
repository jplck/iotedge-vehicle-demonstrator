using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Shared;

namespace locationmodule
{

    interface IModuleTwin
    {
        void UpdatedDesiredPropertiesReceived(ModuleTwin twin);
    }

    class ModuleTwin
    {
        public string RouteFile { get; set; }
        public int UpdateInterval { get; set; }

        public bool SimulationStatus { get; set; }

        private string _routeFileDefault = "routes/gpx2.gpx";
        private int _updateIntervalDefault = 1000;

        private ModuleClient _client;

        private IModuleTwin _delegate;

        public ModuleTwin(ModuleClient client, IModuleTwin twinDelegate)
        {
            _client = client;
            RouteFile = "routes/gpx1.gpx";
            UpdateInterval = 1000;
            SimulationStatus = true;
            _delegate = twinDelegate;
        }

        public async Task Init()
        {
            await _client.SetDesiredPropertyUpdateCallbackAsync(DesiredPropertiesCallback, null);
            await LoadTwin();
        }

        private async Task LoadTwin()
        {
            var twin = await _client.GetTwinAsync();
            await DesiredPropertiesCallback(twin.Properties.Desired, null);
        }

        private async Task DesiredPropertiesCallback(TwinCollection desiredProperties, object userContext)
        {
            RouteFile = !desiredProperties.Contains("RouteFile") ? _routeFileDefault : desiredProperties["RouteFile"];
            UpdateInterval = !desiredProperties.Contains("UpdateInterval") ? _updateIntervalDefault : desiredProperties["UpdateInterval"];

            await SendReportedProperties();
            if (_delegate != null)
            {
                _delegate.UpdatedDesiredPropertiesReceived(this);
            }
        }

        public async Task SendReportedProperties()
        {
            TwinCollection reportedProperties = new TwinCollection
            {
                ["RouteFile"] = RouteFile,
                ["UpdateInterval"] = UpdateInterval,
                ["SimulationStatus"] = SimulationStatus
            };
            await _client.UpdateReportedPropertiesAsync(reportedProperties);
        }
    }
}
