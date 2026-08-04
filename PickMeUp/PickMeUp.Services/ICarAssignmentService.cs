using PickMeUp.Model.Requests;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;

namespace PickMeUp.Services
{
    public interface ICarAssignmentService
        : IBaseCRUDService<CarAssignmentResponse, CarAssignmentSearch, CarAssignmentInsertRequest, CarAssignmentUpdateRequest>
    {
        Task<CarAssignmentResponse> CloseAsync(int id);
    }
}
