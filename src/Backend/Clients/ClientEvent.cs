using System;

namespace Backend.Clients
{
    public class ClientEvent
    {
        public ClientEvent(Guid clientId, string message)
        {
            ClientId = clientId;
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public Guid ClientId { get; set; }

        public string Message { get; set; }
    }
}