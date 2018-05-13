using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;

namespace Backend.Queues.Redis
{
    public class TestSubscriber : Subscriber
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public TestSubscriber(
            IOptionsSnapshot<RedisConfig> options,
            IConnection<ISubscriber> conn,
            ILogger<Subscriber> logger,
            IServiceScopeFactory scopeFactory)
            : base(options, conn, logger)
            => _scopeFactory = scopeFactory;

        protected override Action<string> Handler => (@event) =>
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<TestSubscriber>>();
                logger.LogInformation($"Event received: {@event}");
            }
        };
    }
}