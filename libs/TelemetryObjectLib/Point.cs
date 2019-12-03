using AzureMapsToolkit.Common;
using Newtonsoft.Json;

namespace VehicleDemonstrator.Shared.Telemetry.Location
{
    public class Point : TelemetrySegment
    {
        [JsonProperty("latitude")]
        double _lat;
        [JsonProperty("longitude")]
        double _lon;

        public Point(double lat, double lon) : base(TelemetryType.Coordinates)
        {
            _lat = lat;
            _lon = lon;
        }

        public Point(Coordinate coord) : base(TelemetryType.Coordinates)
        {
            _lat = coord.Latitude;
            _lon = coord.Longitude;
        }

        public Point() : base(TelemetryType.Coordinates)
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
