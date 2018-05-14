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
            using (var socket = new ClientWebSocket())
            {
                socket.Options.SetRequestHeader("X-Request-ID", Guid.NewGuid().ToString());
                await socket.ConnectAsync(new Uri($"ws://{args[0]}"), CancellationToken.None);
                Console.WriteLine("Connection acquired");

                var buffer = new byte[1024 * 4];
                for (var i = 1; i <= 10; i++)
                {
                    var messageString = $"Message number {i}";
                    var message = Encoding.UTF8.GetBytes(messageString);
                    await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(messageString), 0, Encoding.UTF8.GetByteCount(messageString)), WebSocketMessageType.Binary, true, CancellationToken.None);
                    Console.WriteLine("Sent: {0}", messageString);

                    var response = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    Console.WriteLine("Received: {0}", Encoding.UTF8.GetString(Compact(buffer, response.Count)));
                    Thread.Sleep(1000);
                }

                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                Console.WriteLine("Connection closed");
            }

            Console.WriteLine("Client shutting down");
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