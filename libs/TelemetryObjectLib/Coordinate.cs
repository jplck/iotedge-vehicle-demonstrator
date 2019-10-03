using Newtonsoft.Json;
using VehicleDemonstrator.Shared.GPX;

namespace VehicleDemonstrator.Shared.Telemetry.Location
{
    public class Coordinate : TelemetrySegment
    {
        [JsonProperty("latitude")]
        double _Lat;
        [JsonProperty("longitude")]
        double _Lon;
        [JsonProperty("tripGuid")]
        string _tripGuid;

        public Coordinate(double lat, double lon, string tripGuid) : base(TelemetryType.Location)
        {
            _Lat = lat;
            _Lon = lon;
            _tripGuid = tripGuid;
        }

        public Coordinate(GPXGenericItem item, string tripGuid) : base(TelemetryType.Location)
        {
            _Lat = item.Lat;
            _Lon = item.Lon;
            _tripGuid = tripGuid;
        }

        public Coordinate() : base(TelemetryType.Location)
        {
            _Lat = 0;
            _Lon = 0;
            _tripGuid = "";
        }

        public double GetLatitude()
        {
            return _Lat;
        }

        public double GetLongitude()
        {
            return _Lon;
        }
    }
}
