using PickMeUp.Model.Requests;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;

namespace PickMeUp.Services
{
    public interface ITaxiCompanyService
        : IBaseCRUDService<TaxiCompanyResponse, TaxiCompanySearch, TaxiCompanyInsertRequest, TaxiCompanyUpdateRequest>
    {
        Task<TaxiCompanyResponse> GetOwnAsync();

        Task<TaxiCompanyResponse> UpdateOwnAsync(TaxiCompanyUpdateRequest request);

        Task<string> StoreOwnLogoAsync(Stream content, string fileName, string contentType);

        Task<string> StoreLogoAsync(int id, Stream content, string fileName, string contentType);

        Task<(Stream Content, string ContentType, string FileName)> GetLogoAsync(int id);
    }
}
