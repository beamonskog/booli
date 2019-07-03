using Common.Models;
using System;

namespace Common
{
    public static class Calculations
    {
        public static double PositionsToMetersDistance(Position p1, Position p2)
        {
            return CoordinatesToMeters(p1.Longitude, p1.Latitude, p2.Longitude, p2.Latitude);
        }

        private static double CoordinatesToMeters(double longitude, double latitude, double otherLongitude, double otherLatitude)
        {
            var d1 = latitude * (Math.PI / 180.0);
            var num1 = longitude * (Math.PI / 180.0);
            var d2 = otherLatitude * (Math.PI / 180.0);
            var num2 = otherLongitude * (Math.PI / 180.0) - num1;
            var d3 = Math.Pow(Math.Sin((d2 - d1) / 2.0), 2.0) + Math.Cos(d1) * Math.Cos(d2) * Math.Pow(Math.Sin(num2 / 2.0), 2.0);

            var distanceMeters = 6376500.0 * (2.0 * Math.Atan2(Math.Sqrt(d3), Math.Sqrt(1.0 - d3)));
            return distanceMeters;
        }
    }
}
