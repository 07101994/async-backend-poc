using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var clientId = Guid.NewGuid();
            var serviceProvider = ConfigureServices();
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            using (var socket = new ClientWebSocket())
            {
                socket.Options.SetRequestHeader("X-Request-ID", clientId.ToString());
                await socket.ConnectAsync(new Uri($"ws://{args[0]}"), CancellationToken.None);
                logger.LogInformation($"{clientId}|Connection acquired");

                var buffer = new byte[1024 * 4];
                for (var i = 1; i <= 10; i++)
                {
                    var messageId = Guid.NewGuid();
                    await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(messageId.ToString()), 0, Encoding.UTF8.GetByteCount(messageId.ToString())), WebSocketMessageType.Binary, true, CancellationToken.None);
                    logger.LogInformation($"{clientId}|Sent message: {messageId}");

                    var response = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    var responseMessage = Encoding.UTF8.GetString(Compact(buffer, response.Count));

                    if (messageId == Guid.Parse(responseMessage))
                        logger.LogInformation($"{clientId}|Received ok: {responseMessage}");
                    else
                        logger.LogError($"{clientId}|Received error: {responseMessage}");
                }

                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                logger.LogInformation($"{clientId}|Connection closed");
            }

            logger.LogInformation($"{clientId}|Client shutting down");
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection()
                .AddSingleton<ILoggerFactory, LoggerFactory>()
                .AddSingleton(typeof(ILogger<>), typeof(Logger<>))
                .AddLogging((builder) => builder.SetMinimumLevel(LogLevel.Debug));

            var serviceProvider = services.BuildServiceProvider();

            var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

            loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });
            NLog.LogManager.LoadConfiguration("nlog.config");

            return serviceProvider;
        }

        private static byte[] Compact(byte[] array, int count)
        {
            var compacted = new byte[count];
            for (var i = 0; i < count; i++)
                compacted[i] = array[i];

            return compacted;
        }
    }
}