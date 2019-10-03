using System;
using System.Collections.Generic;
using System.Text;

namespace VehicleServices
{
    class Location
    {
        public double Latitude { get; }
        public double Longitude { get; }

        public Location(double lat, double lon)
        {
            Latitude = lat;
            Longitude = lon;
        }

    }
}
