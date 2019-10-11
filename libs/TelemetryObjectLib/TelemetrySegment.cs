using System.Text;
using Newtonsoft.Json;

namespace VehicleDemonstrator.Shared.Telemetry
{

    public enum TelemetryType
    {
        Odometry,
        Coordinates,
        Container,
        Trip
    }

    public class TelemetrySegment
    {
        [JsonProperty("type")]
        private TelemetryType _telemetryType;

        public TelemetrySegment(TelemetryType telemetryType)
        {
            _telemetryType = telemetryType;
        }

        public string ToJson()
        {
            string jsonCoords = JsonConvert.SerializeObject(this);
            return jsonCoords;
        }

        public byte[] ToJsonByteArray()
        {
            byte[] bytes = Encoding.UTF8.GetBytes(ToJson());
            return bytes;
        }

        public TelemetryType GetTelemetryType()
        {
            return _telemetryType;
        }
    }
}
