using Backend.Queues;
using Backend.Queues.Redis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System;

namespace Backend
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public Startup(IHostingEnvironment env)
        {
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
                .AddSingleton<IConnection<ISubscriber>, Connection>()
                .AddSingleton<TestSubscriber>()
                .AddScoped<Publisher>()
                .BuildServiceProvider();

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.Run(async (context) =>
            {
                await app.ApplicationServices.GetRequiredService<Publisher>().PublishAsync(new { sarasa = "sarasa" });
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}