namespace PickMeUp.Model.SearchObjects
{
    public class RecommendationSearch : BaseSearchObject
    {
        public const int DefaultRecommendationCount = 5;

        public int? CityId { get; set; }

        public RecommendationSearch()
        {
            PageSize = DefaultRecommendationCount;
        }
    }
}
