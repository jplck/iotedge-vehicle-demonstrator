﻿using Microsoft.Azure.Devices.Shared;
using VehicleDemonstrator.Shared.Connectivity;

namespace VehicleDemonstrator.Module.Location
{
    interface IModuleTwin
    {
        void DeviceTwinUpdateReceived(VehicleModuleTwin twin);
    }

    class VehicleModuleTwin : ModuleTwin
    {
        private int _updateIntervalDefault = 1000;
        private IModuleTwin _delegate;

        public string RouteFile { get; set; }
        public int UpdateInterval { get; set; }
        public bool SimulationStatus { get; set; }

        public VehicleModuleTwin(IModuleTwin twinDelegate)
        {
            UpdateInterval = 1000;
            SimulationStatus = true;
            _delegate = twinDelegate;
        }

        public override void UpdatedDesiredPropertiesReceived(TwinCollection desiredProperties)
        {
            UpdateInterval = !desiredProperties.Contains("UpdateInterval") ? _updateIntervalDefault : desiredProperties["UpdateInterval"];

            _delegate.DeviceTwinUpdateReceived(this);
        }

        public override TwinCollection ProvideReportedProperties()
        {
            TwinCollection reportedProperties = new TwinCollection
            {
                ["UpdateInterval"] = UpdateInterval,
                ["SimulationStatus"] = SimulationStatus
            };
            return reportedProperties;
        }
    }
}
