using System;
using VehicleDemonstrator.Shared.Telemetry.Location;

namespace VehicleDemonstrator.Module.Location
{
    /*
     * The code in this class is based on the awesome documentation found 
     * at http://www.movable-type.co.uk/scripts/latlong.html © 2002-2019 Chris Veness.
     * 
     * Above each relevant method you see what algorithm has been used.
     */

    static class Util
    {
        //Earth radius
        private static double _Radius = 6371e3;

        /*
         * Distance -> Spherical Law of Cosines
         */
        public static double CalculateDistance(Coordinate coords1, Coordinate coords2)
        {
            double dLon = ToRad(coords2.GetLongitude() - coords1.GetLongitude());
            double lat1 = ToRad(coords1.GetLatitude());
            double lat2 = ToRad(coords2.GetLatitude());
            double distance = Math.Acos(Math.Sin(lat1) * Math.Sin(lat2) +
                    Math.Cos(lat1) * Math.Cos(lat2) * Math.Cos(dLon)) * _Radius;

            return distance;
        }

        /*
         * Rhumb lines -> Bearing
         */
        private static double CalculateBearing(Coordinate coords1, Coordinate coords2)
        {
            double startLat = ToRad(coords1.GetLatitude());
            double startLon = ToRad(coords1.GetLongitude());
            double endLat = ToRad(coords2.GetLatitude());
            double endLon = ToRad(coords2.GetLongitude());
            double dLon = endLon - startLon;

            var dPhi = Math.Log(Math.Tan(endLat / 2.0 + Math.PI / 4.0) / Math.Tan(startLat / 2.0 + Math.PI / 4.0));

            if (Math.Abs(dLon) > Math.PI)
            {
                dLon = (dLon > 0.0) ? -(2.0 * Math.PI - dLon) : (2.0 * Math.PI + dLon);
            }

            return (ToDeg(Math.Atan2(dLon, dPhi)) + 360.0) % 360.0;
        }

        /**
         * Destination point given distance and bearing from start point.
          */
        static Coordinate CalculateCoordinates(Coordinate coords, double bearing, double distance)
        {
            var angularDistRad = distance / _Radius;
            var bearingRad = ToRad(bearing);
            var latRad = ToRad(coords.GetLatitude());
            var lonRad = ToRad(coords.GetLongitude());

            var lat = Math.Asin(Math.Sin(latRad) * Math.Cos(angularDistRad) +
              Math.Cos(latRad) * Math.Sin(angularDistRad) * Math.Cos(bearingRad));

            var lonNormalized = lonRad + Math.Atan2(Math.Sin(bearingRad) * Math.Sin(angularDistRad) * Math.Cos(latRad),
              Math.Cos(angularDistRad) - Math.Sin(latRad) * Math.Sin(lat));
            
            lonNormalized = (lonNormalized + 3 * Math.PI) % (2 * Math.PI) - Math.PI;

            double newLat = ToDeg(lat);
            double newLon = ToDeg(lonNormalized);

            return new Coordinate(newLat, newLon);
        }

        public static Coordinate NewCoordinates(Coordinate start, Coordinate end, double percent)
        {
            double totalDistance = CalculateDistance(start, end);
            double distance = percent * totalDistance;
            double bearing = CalculateBearing(start, end);
            var newCoords = CalculateCoordinates(start, bearing, distance);

            return newCoords;
        }

        private static double ToDeg(double n)
        {
            return n * 180 / Math.PI;
        }

        private static double ToRad(double n)
        {
            return n * Math.PI / 180;
        }

    }
}
