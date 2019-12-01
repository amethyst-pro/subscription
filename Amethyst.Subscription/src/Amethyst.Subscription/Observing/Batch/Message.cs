using System;
using Confluent.Kafka;

namespace Amethyst.Subscription.Observing.Batch
{
    public sealed class Message
    {
        public TopicPartitionOffset Offset { get; }
        public object Event { get; }
        public Guid Key { get; }

        public Message(TopicPartitionOffset offset, Guid key, object @event)
        {
            Offset = offset;
            Event = @event;
            Key = key;
        }
    }
}