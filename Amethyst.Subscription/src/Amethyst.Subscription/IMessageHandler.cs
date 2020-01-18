using System.Threading;
using System.Threading.Tasks;

namespace Amethyst.Subscription
{
    public interface IMessageHandler
    {
        Task HandleAsync<T>(T message, CancellationToken token);
    }
}