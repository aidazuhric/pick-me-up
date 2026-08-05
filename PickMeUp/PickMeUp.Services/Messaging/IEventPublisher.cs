using PickMeUp.Model.Messaging;

namespace PickMeUp.Services.Messaging
{
    public interface IEventPublisher
    {
        Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
    }
}
