using PickMeUp.Common.Services.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace PickMeUp.Services.Messaging
{
    public class RabbitMqConnection : IRabbitMqConnection
    {
        private readonly RabbitMqOptions _options;
        private readonly ILogger<RabbitMqConnection> _logger;
        private readonly SemaphoreSlim _gate = new(1, 1);

        private IConnection? _connection;

        public RabbitMqConnection(IOptions<RabbitMqOptions> options, ILogger<RabbitMqConnection> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task<IChannel> OpenChannelAsync(CancellationToken cancellationToken = default)
        {
            var connection = await GetConnectionAsync(cancellationToken);
            return await connection.CreateChannelAsync(cancellationToken: cancellationToken);
        }

        private async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken)
        {
            if (_connection is { IsOpen: true })
            {
                return _connection;
            }

            await _gate.WaitAsync(cancellationToken);

            try
            {
                if (_connection is { IsOpen: true })
                {
                    return _connection;
                }

                var factory = new ConnectionFactory
                {
                    HostName = _options.Host,
                    Port = _options.Port,
                    UserName = _options.Username,
                    Password = _options.Password,
                    AutomaticRecoveryEnabled = true
                };

                _connection = await factory.CreateConnectionAsync(cancellationToken);
                _logger.LogInformation("Opened RabbitMQ connection to {Host}:{Port}", _options.Host, _options.Port);

                return _connection;
            }
            finally
            {
                _gate.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection != null)
            {
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
                _connection = null;
            }

            _gate.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
