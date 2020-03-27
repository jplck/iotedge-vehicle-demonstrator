using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace VehicleDemonstrator.Shared.Telemetry
{
    public class TelemetrySegmentFactory<T>
    {
        public static T FromJsonString(string json)
        {
            var deserializedObj = JsonConvert.DeserializeObject<T>(json);
            return deserializedObj;
        }

    }
}
