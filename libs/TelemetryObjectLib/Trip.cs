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
        private Coordinate _coords;
        [JsonProperty("tripDistance")]
        private double _tripDistance;
        [JsonProperty("tripTime")]
        private double _tripTime;
        [JsonProperty("tripGuid")]
        string _tripGuid;

        public Trip(string tripGuid, Coordinate coords, double tripDistance, double tripTime) : base(TelemetryType.Trip)
        {
            _coords = coords;
            _tripDistance = tripDistance;
            _tripTime = tripTime;
            _tripGuid = tripGuid;
        }

        public double GetTripDistance()
        {
            return _tripDistance;
        }

        public Coordinate GetCoordinates()
        {
            return _coords;
        }

        public double GetTripTime()
        {
            return _tripTime;
        }
    }
}
