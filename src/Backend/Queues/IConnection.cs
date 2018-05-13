using System;
using System.Threading.Tasks;

namespace Backend.Queues
{
    public interface IConnection<TSession> : IDisposable
    {
        bool IsConnected { get; }

        Task<bool> ConnectAsync();

        Task<bool> TryConnectAsync();

        TSession GetSession();
    }
}