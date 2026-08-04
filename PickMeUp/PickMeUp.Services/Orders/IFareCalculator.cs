namespace PickMeUp.Services.Orders
{
    public interface IFareCalculator
    {
        decimal Calculate(decimal startingPrice, decimal pricePerKm, decimal? distanceKm);

        decimal RoundDistance(decimal distanceKm);
    }
}
