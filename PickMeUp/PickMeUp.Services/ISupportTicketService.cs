using PickMeUp.Model.Requests;
using PickMeUp.Model.Responses;
using PickMeUp.Model.SearchObjects;

namespace PickMeUp.Services
{
    public interface ISupportTicketService : IBaseReadService<SupportTicketResponse, SupportTicketSearch>
    {
        Task<SupportTicketResponse> SubmitAsync(SupportTicketInsertRequest request);

        Task<SupportTicketResponse> ReplyAsync(int id, SupportTicketReplyRequest request);

        Task<SupportTicketResponse> ChangeStatusAsync(int id, SupportTicketStatusChangeRequest request);
    }
}
