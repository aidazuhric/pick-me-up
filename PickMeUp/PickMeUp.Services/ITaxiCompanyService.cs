using PickMeUp.Model.Requests;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;

namespace PickMeUp.Services
{
    public interface ITaxiCompanyService
        : IBaseCRUDService<TaxiCompanyResponse, TaxiCompanySearch, TaxiCompanyInsertRequest, TaxiCompanyUpdateRequest>
    {
        Task<string> StoreLogoAsync(int id, Stream content, string fileName, string contentType);

        Task<(Stream Content, string ContentType, string FileName)> GetLogoAsync(int id);
    }
}
