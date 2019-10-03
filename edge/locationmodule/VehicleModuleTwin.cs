using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;

namespace locationmodule
{
    interface IModuleTwin
    {
        void DeviceTwinUpdateReceived(VehicleModuleTwin twin);
    }

    class VehicleModuleTwin : ModuleTwin
    {
        private string _routeFileDefault = "routes/gpx2.gpx";
        private int _updateIntervalDefault = 1000;
        private IModuleTwin _delegate;

        public string RouteFile { get; set; }
        public int UpdateInterval { get; set; }
        public bool SimulationStatus { get; set; }

        public VehicleModuleTwin(IModuleTwin twinDelegate)
        {
            RouteFile = _routeFileDefault;
            UpdateInterval = 1000;
            SimulationStatus = true;
            _delegate = twinDelegate;
        }

        public override void UpdatedDesiredPropertiesReceived(TwinCollection desiredProperties)
        {
            RouteFile = !desiredProperties.Contains("RouteFile") ? _routeFileDefault : desiredProperties["RouteFile"];
            UpdateInterval = !desiredProperties.Contains("UpdateInterval") ? _updateIntervalDefault : desiredProperties["UpdateInterval"];

            _delegate.DeviceTwinUpdateReceived(this);
        }

        public override TwinCollection ProvideReportedProperties()
        {
            TwinCollection reportedProperties = new TwinCollection
            {
                ["RouteFile"] = RouteFile,
                ["UpdateInterval"] = UpdateInterval,
                ["SimulationStatus"] = SimulationStatus
            };
            return reportedProperties;
        }
    }
}
