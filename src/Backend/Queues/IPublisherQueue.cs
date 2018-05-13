using System.Threading.Tasks;

namespace Backend.Queues
{
    public interface IPublisherQueue
    {
        Task PublishAsync(object @event);
    }
}