using PickMeUp.Model.Requests;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;

namespace PickMeUp.Services
{
    public interface IDriverService
        : IBaseCRUDService<DriverResponse, DriverSearch, DriverInsertRequest, DriverUpdateRequest>
    {
        Task<DriverResponse> ResignAsync(int id);
    }
}
