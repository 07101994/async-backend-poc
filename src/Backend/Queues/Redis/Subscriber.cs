using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Backend.Queues.Redis
{
    public abstract class Subscriber
    {
        private readonly string _channel;
        private readonly IConnection<ISubscriber> _conn;
        private readonly ILogger<Subscriber> _logger;

        public Subscriber(IOptionsSnapshot<RedisConfig> options, IConnection<ISubscriber> conn, ILogger<Subscriber> logger)
        {
            _conn = conn ?? throw new ArgumentNullException(nameof(conn));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (options == null || string.IsNullOrEmpty(options.Value.Channel)) throw new ArgumentNullException(nameof(options));
            _channel = options.Value.Channel;
        }

        protected abstract Action<string> Handler { get; }

        public virtual async Task SubscribeAsync()
        {
            if (!await _conn.TryConnectAsync()) throw new Exception("Cannot connect to Redis");

            var sub = _conn.GetSession();
            await sub.SubscribeAsync(_channel, (channel, value) => Handler(value));

            _logger.LogInformation($"Subscribed to channel {_channel}");
        }
    }
}