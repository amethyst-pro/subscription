using System.Threading;
using System.Threading.Tasks;

namespace Amethyst.Subscription
{
    public interface IEventHandler
    {
        Task Handle<T>(T @event, CancellationToken token);
    }
}