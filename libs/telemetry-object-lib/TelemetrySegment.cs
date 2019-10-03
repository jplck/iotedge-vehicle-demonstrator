using System.Text;
using Newtonsoft.Json;

namespace TelemetryLib
{

    public enum TelemetryType
    {
        Odometry,
        Location,
        Container
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
