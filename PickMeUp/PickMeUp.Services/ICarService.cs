using PickMeUp.Model.Requests;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;

namespace PickMeUp.Services
{
    public interface ICarService
        : IBaseCRUDService<CarResponse, CarSearch, CarInsertRequest, CarUpdateRequest>
    {
        Task<string> StorePhotoAsync(int id, Stream content, string fileName, string contentType);

        Task<(Stream Content, string ContentType, string FileName)> GetPhotoAsync(int id);
    }
}
