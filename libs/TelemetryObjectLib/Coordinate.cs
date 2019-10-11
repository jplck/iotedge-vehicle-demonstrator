using Newtonsoft.Json;
using VehicleDemonstrator.Shared.GPX;

namespace VehicleDemonstrator.Shared.Telemetry.Location
{
    public class Coordinate : TelemetrySegment
    {
        [JsonProperty("latitude")]
        double _lat;
        [JsonProperty("longitude")]
        double _lon;

        public Coordinate(double lat, double lon) : base(TelemetryType.Coordinates)
        {
            _lat = lat;
            _lon = lon;
        }

        public Coordinate(GPXGenericItem item) : base(TelemetryType.Coordinates)
        {
            _lat = item.Lat;
            _lon = item.Lon;
        }

        public Coordinate() : base(TelemetryType.Coordinates)
        {
            _lat = 0;
            _lon = 0;
        }

        public double GetLatitude()
        {
            return _lat;
        }

        public double GetLongitude()
        {
            return _lon;
        }
    }
}
