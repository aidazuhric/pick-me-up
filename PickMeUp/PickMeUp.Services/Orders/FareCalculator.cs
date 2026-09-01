namespace PickMeUp.Services.Orders
{
    public class FareCalculator : IFareCalculator
    {
        public const int DistanceDecimals = 2;

        public decimal Calculate(decimal startingPrice, decimal pricePerKm, decimal? distanceKm)
        {
            var distance = distanceKm.HasValue ? RoundDistance(distanceKm.Value) : 0m;

            if (distance < 0m)
            {
                distance = 0m;
            }

            var fare = startingPrice + (distance * pricePerKm);

            return Math.Round(Math.Max(fare, startingPrice), 2, MidpointRounding.AwayFromZero);
        }

        public decimal RoundDistance(decimal distanceKm) =>
            Math.Round(distanceKm, DistanceDecimals, MidpointRounding.AwayFromZero);
    }
}
