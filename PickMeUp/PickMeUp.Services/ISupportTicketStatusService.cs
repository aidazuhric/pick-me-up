using PickMeUp.Model.Requests;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;

namespace PickMeUp.Services
{
    public interface ISupportTicketStatusService
        : IBaseCRUDService<SupportTicketStatusResponse, SupportTicketStatusSearch, SupportTicketStatusInsertRequest, SupportTicketStatusUpdateRequest>
    {
    }
}
