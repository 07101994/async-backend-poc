using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Backend
{
    public interface IConnection<TConnection>
    {
        Task ReceiveAsync(Guid id, TConnection conn);

        Task SendAsync(Guid id, string message);
    }

    public interface IConnection : IConnection<WebSocket>
    { }
}