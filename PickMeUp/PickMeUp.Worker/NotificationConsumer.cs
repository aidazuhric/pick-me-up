using PickMeUp.Common.Services.Configuration;
using PickMeUp.Model.Messaging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace PickMeUp.Worker
{
    public class NotificationConsumer : BackgroundService
    {
        private static readonly TimeSpan[] RetryDelays =
        {
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(4),
            TimeSpan.FromSeconds(8)
        };

        private readonly RabbitMqOptions _options;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<NotificationConsumer> _logger;

        private IConnection? _connection;
        private IChannel? _channel;

        public NotificationConsumer(
            IOptions<RabbitMqOptions> options,
            IEmailSender emailSender,
            ILogger<NotificationConsumer> logger)
        {
            _options = options.Value;
            _emailSender = emailSender;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await ConnectWithRetryAsync(stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel!);
            consumer.ReceivedAsync += OnReceivedAsync;

            await _channel!.BasicConsumeAsync(MessagingTopology.NotificationQueue, autoAck: false, consumer, stoppingToken);

            _logger.LogInformation("Worker is consuming {Queue}", MessagingTopology.NotificationQueue);

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        private async Task OnReceivedAsync(object sender, BasicDeliverEventArgs args)
        {
            EventEnvelope? envelope = null;

            try
            {
                envelope = JsonSerializer.Deserialize<EventEnvelope>(Encoding.UTF8.GetString(args.Body.Span));
            }
            catch (JsonException exception)
            {
                _logger.LogError(exception, "Discarding a message that is not a valid event envelope.");
                await _channel!.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false);
                return;
            }

            if (envelope == null)
            {
                _logger.LogError("Discarding an empty message body.");
                await _channel!.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false);
                return;
            }

            _logger.LogInformation("Consumed {RoutingKey} event {EventId} for {Recipient}",
                envelope.RoutingKey, envelope.EventId, envelope.RecipientEmail);

            var delivered = await SendWithBackoffAsync(envelope);

            if (delivered)
            {
                await _channel!.BasicAckAsync(args.DeliveryTag, multiple: false);
            }
            else
            {
                await _channel!.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false);
            }
        }

        private async Task<bool> SendWithBackoffAsync(EventEnvelope envelope)
        {
            for (var attempt = 0; attempt <= RetryDelays.Length; attempt++)
            {
                try
                {
                    await _emailSender.SendAsync(envelope.RecipientEmail, envelope.RecipientName,
                        envelope.Subject, envelope.Body, envelope.HtmlBody, CancellationToken.None);

                    _logger.LogInformation("Delivered event {EventId} to {Recipient} on attempt {Attempt}",
                        envelope.EventId, envelope.RecipientEmail, attempt + 1);

                    return true;
                }
                catch (Exception exception)
                {
                    if (attempt == RetryDelays.Length)
                    {
                        _logger.LogError(exception,
                            "Giving up on event {EventId} for {Recipient} after {Attempts} attempts.",
                            envelope.EventId, envelope.RecipientEmail, attempt + 1);

                        return false;
                    }

                    var delay = RetryDelays[attempt];

                    _logger.LogWarning(exception,
                        "Attempt {Attempt} to deliver event {EventId} failed. Retrying in {Delay}s.",
                        attempt + 1, envelope.EventId, delay.TotalSeconds);

                    await Task.Delay(delay);
                }
            }

            return false;
        }

        private async Task ConnectWithRetryAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.Host,
                Port = _options.Port,
                UserName = _options.Username,
                Password = _options.Password,
                AutomaticRecoveryEnabled = true
            };

            var attempt = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _connection = await factory.CreateConnectionAsync(stoppingToken);
                    _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

                    await _channel.ExchangeDeclareAsync(MessagingTopology.Exchange, ExchangeType.Topic,
                        durable: true, cancellationToken: stoppingToken);

                    await _channel.QueueDeclareAsync(MessagingTopology.NotificationQueue, durable: true,
                        exclusive: false, autoDelete: false, cancellationToken: stoppingToken);

                    await _channel.QueueBindAsync(MessagingTopology.NotificationQueue, MessagingTopology.Exchange,
                        routingKey: "#", cancellationToken: stoppingToken);

                    await _channel.BasicQosAsync(0, prefetchCount: 10, global: false, cancellationToken: stoppingToken);

                    _logger.LogInformation("Worker connected to RabbitMQ at {Host}:{Port}", _options.Host, _options.Port);
                    return;
                }
                catch (Exception exception)
                {
                    attempt++;
                    var delay = RetryDelays[Math.Min(attempt - 1, RetryDelays.Length - 1)];

                    _logger.LogWarning(exception,
                        "Could not reach RabbitMQ at {Host}:{Port} (attempt {Attempt}). Retrying in {Delay}s.",
                        _options.Host, _options.Port, attempt, delay.TotalSeconds);

                    await Task.Delay(delay, stoppingToken);
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel != null)
            {
                await _channel.CloseAsync(cancellationToken);
                await _channel.DisposeAsync();
            }

            if (_connection != null)
            {
                await _connection.CloseAsync(cancellationToken);
                await _connection.DisposeAsync();
            }

            await base.StopAsync(cancellationToken);
        }
    }
}
