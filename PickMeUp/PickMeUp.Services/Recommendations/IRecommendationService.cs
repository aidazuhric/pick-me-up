using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;

namespace PickMeUp.Services.Recommendations
{
    public interface IRecommendationService
    {
        Task<PageResult<RecommendationResponse>> GetForCurrentPassengerAsync(RecommendationSearch? search = null);
    }
}
