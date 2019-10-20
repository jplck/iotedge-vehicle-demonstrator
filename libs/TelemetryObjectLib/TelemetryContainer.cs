using Newtonsoft.Json;

namespace VehicleDemonstrator.Shared.Telemetry
{
    public class TelemetryContainer<T>: TelemetrySegment
    {
        [JsonProperty("deviceId")]
        private string _deviceId;
        [JsonProperty("payload")]
        private T _segment;

        public TelemetryContainer(string deviceId, T segment) : base(TelemetryType.Container)
        {
            _deviceId = deviceId;
            _segment = segment;
        }

        

        public string GetDeviceId()
        {
            return _deviceId;
        }

        public T GetPayload()
        {
            return _segment;
        }
    }
}
