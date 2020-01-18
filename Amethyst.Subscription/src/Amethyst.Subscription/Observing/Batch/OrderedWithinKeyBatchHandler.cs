using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Amethyst.Subscription.Observing.Batch
{
    public sealed class OrderedWithinKeyBatchHandler : IBatchHandler
    {
        private readonly SingleTypeBatchHandler _singleTypeHandler;

        public OrderedWithinKeyBatchHandler(SingleTypeBatchHandler handler)
        {
            _singleTypeHandler = handler;
        }

        public async Task HandleAsync(Message[] messages, CancellationToken token)
        {
            if (AllMessagesHasSameType(messages))
            {
                await _singleTypeHandler.HandleAsync(messages, token);
                return;
            }

            var remainingEvents = new List<Message>(messages);

            while (remainingEvents.Count > 0)
            {
                var generation = new Dictionary<Guid, Message>(remainingEvents.Count / 2 + 1);
                var nextGenerationMessages = new List<Message>(remainingEvents.Count / 2);

                foreach (var message in remainingEvents)
                {
                    if (!generation.TryAdd(message.Key, message))
                        nextGenerationMessages.Add(message);
                }

                remainingEvents = nextGenerationMessages;

                await HandleGeneration(generation.Values, token);
            }
        }

        private static bool AllMessagesHasSameType(Message[] messages)
        {
            var sampleType = messages[^1].Event.GetType();

            return Array.TrueForAll(messages, m => m.Event.GetType() == sampleType);
        }

        private async Task HandleGeneration(IEnumerable<Message> messages, CancellationToken token)
        {
            var groupedByTypeEvents = messages
                .GroupBy(m => m.Event.GetType());

            foreach (var group in groupedByTypeEvents)
                await _singleTypeHandler.HandleAsync(@group.ToArray(), token);
        }
    }
}