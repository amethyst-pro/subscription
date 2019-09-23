using System;
using Amethyst.Subscription.Abstractions;
using Amethyst.Subscription.Broker.Exceptions;
using Confluent.Kafka;

namespace Amethyst.Subscription.Broker
{
    public sealed class ValueObjectDeserializer : IDeserializer<IStreamEvent>
    {
        private readonly IEventDeserializer _deserializer;

        public ValueObjectDeserializer(IEventDeserializer deserializer)
        {
            _deserializer = deserializer;
        }

        public IStreamEvent Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            try
            {
                return _deserializer.Deserialize(data);
            }
            catch (Exception ex)
            {
                throw new InvalidEventException(
                    context.Topic,
                    $"Can't deserialize event. Deserializer type {_deserializer.GetType().Name}",
                    ex);
            }
        }
    }
}