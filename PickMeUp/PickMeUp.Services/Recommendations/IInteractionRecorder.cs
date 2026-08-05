using PickMeUp.Model.Enums;

namespace PickMeUp.Services.Recommendations
{
    public interface IInteractionRecorder
    {
        Task RecordAsync(
            int userId,
            int taxiCompanyId,
            InteractionSignalType signalType,
            decimal value,
            int? orderId = null,
            CancellationToken cancellationToken = default);
    }
}
