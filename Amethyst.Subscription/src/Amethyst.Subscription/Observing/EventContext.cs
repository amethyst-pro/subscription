using System;
using System.Collections.Generic;
using System.Linq;
using Amethyst.Subscription.Abstractions;
using Amethyst.Subscription.Broker;
using Confluent.Kafka;

namespace Amethyst.Subscription.Observing
{
    public readonly struct EventContext : IEventContext
    {
        private readonly ConsumeResult<Guid, IStreamEvent> _event;

        public EventContext(ConsumeResult<Guid, IStreamEvent> @event)
        {
            _event = @event ?? throw new ArgumentNullException(nameof(@event));
        }

        public TopicPartitionOffset Offset => _event.TopicPartitionOffset;

        public void Acknowledge(IConsumer consumer) => consumer.Commit(Offset);

        public object GetEvent() => _event.Value.Event ?? throw new InvalidOperationException("Unknown event consumed");

        public DeserializationStatus Status => _event.Value.Status;

        public IReadOnlyCollection<KeyValuePair<string, object>> GetMetadata()
        {
            var offset = new KeyValuePair<string, object>("md_kafka_offset", Offset);
            var metadata = _event.Value.Metadata;

            if (metadata?.Count > 0)
                return metadata.Append(offset).ToArray();

            var result = Status;
            var resultPair = new KeyValuePair<string, object>("md_serialization_result", result);

            if (result == DeserializationStatus.Success && _event.Value.Event != null)
            {
                var type = new KeyValuePair<string, object>("md_ev_type", _event.Value.Event.GetType().Name);

                return new[] {offset, resultPair, type};
            }

            return new[] {offset, resultPair};
        }

        public Guid GetKey() => _event.Key;
    }
}