namespace PickMeUp.Services.Orders
{
    /// <summary>
    /// Server-side fare engine. The amount is derived only from the company's price list and
    /// the logged distance; no client value ever reaches it.
    /// </summary>
    public class FareCalculator : IFareCalculator
    {
        public const int DistanceDecimals = 2;

        /// <summary>
        /// Fare is the starting price plus distance times the per-kilometre rate, and never
        /// less than the starting price. A missing or zero distance yields the starting price
        /// alone, which is what a ride cancelled after pickup is charged. Distance is rounded
        /// to two decimals before it is priced, so the same trip always costs the same.
        /// </summary>
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
