using System.Threading.Tasks;

namespace Amethyst.Subscription.Abstractions
{
    public interface IEventHandler<in TEvent>
    {
        Task HandleAsync(TEvent @event);
    }
}