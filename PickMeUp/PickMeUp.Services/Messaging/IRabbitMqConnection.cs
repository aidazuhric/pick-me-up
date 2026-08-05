using RabbitMQ.Client;

namespace PickMeUp.Services.Messaging
{
    public interface IRabbitMqConnection : IAsyncDisposable
    {
        Task<IChannel> OpenChannelAsync(CancellationToken cancellationToken = default);
    }
}
