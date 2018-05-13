using Backend.Clients;
using Backend.Queues;
using Microsoft.Extensions.Logging;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Backend.WebSockets
{
    public class WebSocketConnectionHandler : IConnection
    {
        private readonly WebSocketRoom _room;
        private readonly IPublisherQueue _publisher;
        private readonly ILogger<WebSocketConnectionHandler> _logger;

        public WebSocketConnectionHandler(
            WebSocketRoom room,
            IPublisherQueue publisher,
            ILogger<WebSocketConnectionHandler> logger)
        {
            _room = room ?? throw new ArgumentNullException(nameof(room));
            _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ReceiveAsync(Guid id, WebSocket socket)
        {
            _room.Add(id, socket);

            var buffer = new byte[1024 * 4];
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                _logger.LogInformation($"Websocket message received from client {id}");
                var @event = new ClientEvent(id, Encoding.UTF8.GetString(Compact(buffer, result.Count)));

                await _publisher.PublishAsync(@event);

                result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            _logger.LogInformation($"Websocket close received from client {id}");
            await socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            _room.Remove(id);
        }

        public async Task SendAsync(Guid id, string message)
        {
            var socket = _room.Get(id);
            if (socket == null)
            {
                _logger.LogInformation($"Client {id} does not belog to this server");
                return;
            }

            if (socket.State != WebSocketState.Open)
            {
                _room.Remove(id);
                _logger.LogWarning($"Client {id} already disconnected");
                return;
            }

            var messageBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
            await socket.SendAsync(messageBuffer, WebSocketMessageType.Binary, true, CancellationToken.None);
            _logger.LogInformation($"Websocket message sent to client {id}");
        }

        private byte[] Compact(byte[] array, int count)
        {
            var compacted = new byte[count];
            for (var i = 0; i < count; i++)
                compacted[i] = array[i];

            return compacted;
        }
    }
}