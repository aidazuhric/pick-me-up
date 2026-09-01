using System;

namespace PickMeUp.Services.Orders
{
    public static class GeoDistance
    {
        private const double EarthRadiusKm = 6371.0088;

        public static double KilometresBetween(double fromLat, double fromLng, double toLat, double toLng)
        {
            var lat1 = ToRadians(fromLat);
            var lat2 = ToRadians(toLat);
            var deltaLat = ToRadians(toLat - fromLat);
            var deltaLng = ToRadians(toLng - fromLng);

            var h = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2)
                  + Math.Cos(lat1) * Math.Cos(lat2) * Math.Sin(deltaLng / 2) * Math.Sin(deltaLng / 2);

            return 2 * EarthRadiusKm * Math.Asin(Math.Min(1.0, Math.Sqrt(h)));
        }

        private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
    }
}
