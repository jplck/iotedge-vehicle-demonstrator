using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using VehicleDemonstrator.Shared.Telemetry.Location;

namespace VehicleDemonstrator.Shared.Telemetry
{
    public class Trip : TelemetrySegment
    {
        [JsonProperty("coordinates")]
        private Point _coords;
        [JsonProperty("speedLimit")]
        private double _speedLimit;
        [JsonProperty("tripDistance")]
        private double _tripDistance;
        [JsonProperty("tripTime")]
        private double _tripTime;
        [JsonProperty("tripGuid")]
        string _tripGuid;

        public Trip(string tripGuid, Point coords, double tripDistance, double tripTime, double speedLimit = 0) : base(TelemetryType.Trip)
        {
            _coords = coords;
            _tripDistance = tripDistance;
            _tripTime = tripTime;
            _tripGuid = tripGuid;
            _speedLimit = speedLimit;
        }

        public double GetTripDistance()
        {
            return _tripDistance;
        }

        public Point GetCoordinates()
        {
            return _coords;
        }

        public double GetTripTime()
        {
            return _tripTime;
        }
        public double GetSpeedLimit()
        {
            return _speedLimit;
        }
    }
}
