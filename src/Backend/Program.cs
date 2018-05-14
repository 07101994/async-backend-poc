using Backend.Queues.Redis;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Web;
using System.Threading.Tasks;

namespace Backend
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var host = BuildWebHost(args);
            await host.Services.GetRequiredService<Subscriber>().SubscribeAsync();
            await host.RunAsync();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(new ConfigurationBuilder().AddCommandLine(args).Build())
                .UseStartup<Startup>()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Debug);
                })
                .UseNLog()
                .Build();
    }
}