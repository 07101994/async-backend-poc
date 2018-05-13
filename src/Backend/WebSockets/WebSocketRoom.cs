using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace Backend.WebSockets
{
    public class WebSocketRoom
    {
        private static ConcurrentDictionary<Guid, WebSocket> _sockets = new ConcurrentDictionary<Guid, WebSocket>();

        public WebSocket Get(Guid id)
        {
            _sockets.TryGetValue(id, out WebSocket value);
            return value;
        }

        public void Add(Guid id, WebSocket ws) => _sockets.TryAdd(id, ws);

        public void Remove(Guid id) => _sockets.TryRemove(id, out WebSocket socket);
    }
}