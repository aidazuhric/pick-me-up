using PickMeUp.Model.Requests;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;

namespace PickMeUp.Services
{
    public interface IReviewService
        : IBaseCRUDService<ReviewResponse, ReviewSearch, ReviewInsertRequest, ReviewUpdateRequest>
    {
        Task<List<RatingSummaryResponse>> GetCompanyAveragesAsync();

        Task<List<RatingSummaryResponse>> GetDriverAveragesAsync();
    }
}
