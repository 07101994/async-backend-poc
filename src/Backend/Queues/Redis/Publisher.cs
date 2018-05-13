using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Backend.Queues.Redis
{
    public class Publisher
    {
        private readonly string _channel;
        private readonly IConnection<ISubscriber> _conn;
        private readonly ILogger<Publisher> _logger;

        public Publisher(IOptionsSnapshot<RedisConfig> options, IConnection<ISubscriber> conn, ILogger<Publisher> logger)
        {
            _conn = conn ?? throw new ArgumentNullException(nameof(conn));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (options == null || string.IsNullOrEmpty(options.Value.Channel)) throw new ArgumentNullException(nameof(options));
            _channel = options.Value.Channel;
        }

        public async Task PublishAsync(object @event)
        {
            if (!await _conn.TryConnectAsync()) throw new Exception("Cannot connect to Redis");

            await _conn.GetSession().PublishAsync(_channel, JsonConvert.SerializeObject(@event));
            _logger.LogInformation($"Published event to channel {_channel}");
        }
    }
}