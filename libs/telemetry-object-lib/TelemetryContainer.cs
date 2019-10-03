﻿using Newtonsoft.Json;

namespace TelemetryLib
{
    public class TelemetryContainer: TelemetrySegment
    {
        [JsonProperty("deviceId")]
        private string _deviceId;
        [JsonProperty("payload")]
        private TelemetrySegment _segment;

        public TelemetryContainer(string deviceId, TelemetrySegment segment) : base(TelemetryType.Container)
        {
            _deviceId = deviceId;
            _segment = segment;
        }
    }
}
