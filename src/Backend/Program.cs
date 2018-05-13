using Backend.Queues.Redis;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Backend
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            var host = BuildWebHost(args);

            await host.Services.GetRequiredService<TestSubscriber>().SubscribeAsync();

            await host.RunAsync();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}