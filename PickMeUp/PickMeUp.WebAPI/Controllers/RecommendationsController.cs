using PickMeUp.Model.Constants;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;
using PickMeUp.Services.Recommendations;
using PickMeUp.WebAPI.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PickMeUp.WebAPI.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class RecommendationsController : ControllerBase
{
    private readonly IRecommendationService _service;

    public RecommendationsController(IRecommendationService service)
    {
        _service = service;
    }

    /// <summary>
    /// Taxi companies recommended to the signed-in passenger, best first. The score blends a
    /// content-based part (how often this passenger has ordered from and completed rides with the
    /// company, and how they rated it) with a popularity part (completed rides and average rating
    /// across all passengers), plus a bonus for companies in the city the passenger rides in most.
    /// Every input comes from stored InteractionHistory rows written when a ride is ordered,
    /// completed or reviewed, and each result carries the reason it was recommended along with the
    /// counts behind it. A passenger with no history yet is ranked purely on popularity. The
    /// recommendations are always for the signed-in passenger; there is no way to ask for
    /// another passenger's.
    /// </summary>
    [Authorization(Roles.Passenger)]
    [HttpGet]
    public Task<PageResult<RecommendationResponse>> GetAll([FromQuery] RecommendationSearch? search)
    {
        return _service.GetForCurrentPassengerAsync(search);
    }
}
