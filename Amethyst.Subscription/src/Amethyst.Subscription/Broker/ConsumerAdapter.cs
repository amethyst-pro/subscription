using System;
using System.Linq;
using System.Threading;
using Amethyst.Subscription.Abstractions;
using Confluent.Kafka;

namespace Amethyst.Subscription.Broker
{
    public sealed class ConsumerAdapter : IConsumer
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly IConsumer<Guid, IStreamEvent> _consumer;

        public ConsumerAdapter(CancellationTokenSource cancellationTokenSource, IConsumer<Guid, IStreamEvent> consumer)
        {
            _cancellationTokenSource = cancellationTokenSource;
            _consumer = consumer;
        }

        public void Commit(params TopicPartitionOffset[] offsets)
        {
            var nextOffsets = offsets
                .Select(o => new TopicPartitionOffset(o.TopicPartition, o.Offset + 1));

            _consumer.Commit(nextOffsets);
        }

        public void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}