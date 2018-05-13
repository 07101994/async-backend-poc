using Backend.Clients;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;

namespace Backend.Queues.Redis
{
    public class Subscriber
    {
        private readonly string _channel;
        private readonly IConnection<ISubscriber> _conn;
        private readonly ILogger<Subscriber> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public Subscriber(
            IOptionsSnapshot<RedisConfig> options,
            IConnection<ISubscriber> conn,
            ILogger<Subscriber> logger,
            IServiceScopeFactory scopeFactory)
        {
            _conn = conn ?? throw new ArgumentNullException(nameof(conn));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));

            if (options == null || string.IsNullOrEmpty(options.Value.Channel)) throw new ArgumentNullException(nameof(options));
            _channel = options.Value.Channel;
        }

        protected virtual Action<string> Handler => async (@event) =>
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                _logger.LogInformation($"Event received: {@event}");
                var eventObject = JsonConvert.DeserializeObject<ClientEvent>(@event);
                await scope.ServiceProvider.GetRequiredService<ClientEventHandler>().HandleAsync(eventObject);
            }
        };

        public virtual async Task SubscribeAsync()
        {
            if (!await _conn.TryConnectAsync()) throw new Exception("Cannot connect to Redis");

            await _conn.GetSession().SubscribeAsync(_channel, (channel, value) => Handler(value));
            _logger.LogInformation($"Subscribed to channel {_channel}");
        }
    }
}