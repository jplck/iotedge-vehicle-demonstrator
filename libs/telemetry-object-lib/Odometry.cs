using Newtonsoft.Json;

namespace TelemetryLib
{
    public class Odometry : TelemetrySegment
    {
        [JsonProperty("speed")]
        private int _speed;

        public Odometry(int speed) : base(TelemetryType.Odometry)
        {
            _speed = speed;
        }

        public Odometry() : base(TelemetryType.Odometry)
        {
            _speed = 50;
        }

        public int GetSpeed()
        {
            return _speed;
        }

    }
}
