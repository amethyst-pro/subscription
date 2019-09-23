using System.Threading;
using System.Threading.Tasks;

namespace Amethyst.Subscription
{
    public interface IObserver
    {
        Task OnEventAppeared<T>(T context, CancellationToken token) 
            where T : IEventContext;
    }
}