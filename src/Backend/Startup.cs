using Backend.Queues;
using Backend.Queues.Redis;
using Backend.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;
using System.Net.WebSockets;

namespace Backend
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
            NLog.Web.NLogBuilder.ConfigureNLog($"nlog.config");
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
            => services
                .AddOptions()
                .Configure<RedisConfig>(Configuration.GetSection(nameof(RedisConfig)))
                .AddSingleton<WebSocketRoom>()
                .AddScoped<IConnection, WebSocketConnectionHandler>()
                .AddSingleton<Queues.IConnection<ISubscriber>, Connection>()
                .AddScoped<Clients.ClientEventHandler>()
                .AddSingleton<Subscriber>()
                .AddScoped<IPublisherQueue, Publisher>()
                .BuildServiceProvider();

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app
                .UseWebSockets()
                .Run(async (context) =>
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        var id = context.Request.Headers.ContainsKey("X-Request-ID")
                        ? Guid.Parse(context.Request.Headers["X-Request-ID"])
                        : Guid.NewGuid();
                        await app.ApplicationServices.GetRequiredService<IConnection>().ReceiveAsync(id, webSocket);
                    }
                    else
                        await context.Response.WriteAsync("Hello world!");
                });
        }
    }
}