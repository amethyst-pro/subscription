using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Amethyst.Subscription.Observing.Batch
{
    public sealed class OrderedWithinTypeBatchHandler : IBatchHandler
    {
        private readonly SingleTypeBatchHandler _singleTypeHandler;

        public OrderedWithinTypeBatchHandler(SingleTypeBatchHandler handler)
        {
            _singleTypeHandler = handler;
        }

        public async Task HandleAsync(Message[] messages, CancellationToken token)
        {
            var groupedByTypeEvents = messages
                .GroupBy(m => m.Event.GetType());

            foreach (var group in groupedByTypeEvents)
                await _singleTypeHandler.HandleAsync(@group.ToArray(), token);
        }
    }
}