namespace PickMeUp.Model.Constants
{
    public static class RecommenderWeights
    {
        public const double Content = 0.65;
        public const double Popularity = 0.35;

        public const double ContentCompletedRides = 0.50;
        public const double ContentOrders = 0.20;
        public const double ContentOwnRating = 0.30;

        public const double PopularityCompletedRides = 0.50;
        public const double PopularityRating = 0.50;

        public const double HomeCityBonus = 0.10;

        public const int UsageSaturation = 5;
        public const double NeutralRating = 0.5;
    }
}
