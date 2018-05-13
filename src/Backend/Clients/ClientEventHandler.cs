using System.Threading.Tasks;

namespace Backend.Clients
{
    public class ClientEventHandler
    {
        private readonly Backend.IConnection _conn;

        public ClientEventHandler(Backend.IConnection conn) => _conn = conn;

        public async Task HandleAsync(ClientEvent @event)
            => await _conn.SendAsync(@event.ClientId, @event.Message);
    }
}