using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;

namespace PickMeUp.Services
{
    public interface IShiftService : IBaseReadService<ShiftResponse, ShiftSearch>
    {
        Task<ShiftResponse> StartShiftAsync();

        Task<ShiftResponse> StopShiftAsync(int id);

        Task<ShiftResponse> StartBreakAsync(int id);

        Task<ShiftResponse> StopBreakAsync(int id);

        Task<DriverEligibilityResponse> GetEligibilityAsync(int driverId);

        Task<bool> IsDriverDispatchEligibleAsync(int driverId);

        Task EnsureDriverMayTakeNewWorkAsync(int driverId);

        IQueryable<int> QueryDispatchEligibleDriverIds();

        IQueryable<int> QueryDriverIdsAllowedNewWork();
    }
}
