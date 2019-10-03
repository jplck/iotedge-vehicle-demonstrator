using System;
using System.Collections.Generic;
using System.Text;

namespace VehicleServices
{
    struct Telemetry
    {
        public string VIN { get; set; }
        public DateTime Timestamp;
        public Odometry Odometry;
        public EngineTelemetry EngineTelemetry;
        public Location Location;

    }
}
