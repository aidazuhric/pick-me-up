using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PickMeUp.Services
{
    public interface IBaseReadService<TResponse, TSearch>
        where TSearch : BaseSearchObject
    {
        Task<TResponse> GetByIdAsync(int id);
        Task<PageResult<TResponse>> GetAllAsync(TSearch? search = null);
    }
}
