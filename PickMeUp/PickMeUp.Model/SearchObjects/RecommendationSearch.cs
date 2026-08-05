namespace PickMeUp.Model.SearchObjects
{
    public class RecommendationSearch : BaseSearchObject
    {
        public const int DefaultRecommendationCount = 5;

        public RecommendationSearch()
        {
            PageSize = DefaultRecommendationCount;
        }
    }
}
