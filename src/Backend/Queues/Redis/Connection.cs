using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Backend.Queues.Redis
{
    public class Connection : IConnection<ISubscriber>
    {
        private ConnectionMultiplexer _conn;
        private readonly ILogger<Connection> _logger;
        private readonly ConfigurationOptions _config;

        public Connection(ILogger<Connection> logger, IOptionsSnapshot<RedisConfig> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (options == null || string.IsNullOrEmpty(options.Value.Endpoint)) throw new ArgumentNullException(nameof(options));

            _config = ConfigurationOptions.Parse(options?.Value.Endpoint);
        }

        public bool IsConnected => _conn != null && _conn.IsConnected;

        public async Task<bool> ConnectAsync()
        {
            _conn = await ConnectionMultiplexer.ConnectAsync(_config);
            if (!IsConnected)
            {
                _logger.LogCritical($"Redis: The connection could not be created");
                return false;
            }

            _logger.LogInformation($"Redis: Connection established");
            return true;
        }

        public async Task<bool> TryConnectAsync() => IsConnected ? true : await ConnectAsync();

        public ISubscriber GetSession()
        {
            if (!IsConnected)
                throw new InvalidOperationException("Redis: Cannot get a session from a closed connection");

            return _conn.GetSubscriber();
        }

        public void Dispose()
        {
            try
            {
                _conn.Close();
                _conn.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(
                    exception: ex,
                    message: "Redis: Connection could not be disposed");
            }
        }
    }
}