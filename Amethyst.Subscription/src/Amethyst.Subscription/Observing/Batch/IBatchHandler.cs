using System.Threading;
using System.Threading.Tasks;

namespace Amethyst.Subscription.Observing.Batch
{
    public interface IBatchHandler
    {
        Task HandleAsync(Message[] messages, CancellationToken token);
    }
}