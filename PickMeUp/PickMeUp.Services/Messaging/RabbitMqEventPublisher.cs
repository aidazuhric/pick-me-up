using PickMeUp.Model.Messaging;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace PickMeUp.Services.Messaging
{
    public class RabbitMqEventPublisher : IEventPublisher
    {
        private readonly IRabbitMqConnection _connection;
        private readonly ILogger<RabbitMqEventPublisher> _logger;

        public RabbitMqEventPublisher(IRabbitMqConnection connection, ILogger<RabbitMqEventPublisher> logger)
        {
            _connection = connection;
            _logger = logger;
        }

        public async Task PublishAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
        {
            await using var channel = await _connection.OpenChannelAsync(cancellationToken);

            await channel.ExchangeDeclareAsync(MessagingTopology.Exchange, ExchangeType.Topic, durable: true,
                cancellationToken: cancellationToken);

            var envelope = new EventEnvelope
            {
                RoutingKey = integrationEvent.RoutingKey,
                EventId = integrationEvent.EventId,
                OccurredAt = integrationEvent.OccurredAt,
                RecipientEmail = integrationEvent.RecipientEmail,
                RecipientName = integrationEvent.RecipientName,
                Subject = integrationEvent.Subject,
                Body = integrationEvent.Body,
                HtmlBody = integrationEvent.HtmlBody,
                PayloadJson = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType())
            };

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(envelope));

            await channel.BasicPublishAsync(
                exchange: MessagingTopology.Exchange,
                routingKey: integrationEvent.RoutingKey,
                mandatory: false,
                basicProperties: new BasicProperties { Persistent = true, ContentType = "application/json" },
                body: body,
                cancellationToken: cancellationToken);

            _logger.LogInformation("Published {RoutingKey} event {EventId}", integrationEvent.RoutingKey, integrationEvent.EventId);
        }
    }
}
