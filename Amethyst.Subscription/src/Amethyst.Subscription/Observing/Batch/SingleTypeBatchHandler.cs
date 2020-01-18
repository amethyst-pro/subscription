using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Amethyst.Subscription.Observing.Batch
{
    public sealed class SingleTypeBatchHandler : IBatchHandler
    {
        private readonly IMessageHandler _handler;

        public SingleTypeBatchHandler(IMessageHandler handler)
        {
            _handler = handler;
        }

        public Task HandleAsync(Message[] messages, CancellationToken token)
        {
            return HandleTyped(
                (dynamic) messages[0].Event,
                messages,
                token);
        }

        private Task HandleTyped<T>(T sample, Message[] messages, CancellationToken token)
        {
            return _handler.HandleAsync<IReadOnlyCollection<T>>(
                messages.Select(m => (T) m.Event).ToArray(),
                token);
        }
    }
}