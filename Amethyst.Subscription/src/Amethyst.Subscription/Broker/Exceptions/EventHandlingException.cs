using System;
using Confluent.Kafka;

namespace Amethyst.Subscription.Broker.Exceptions
{
    public class EventHandlingException : Exception
    {
        public EventHandlingException(TopicPartitionOffset offset, string message, Exception innerException)
            : base($"{message} Topic: '{offset.Topic}', Partition: {offset.Partition}, Offset: {offset.Offset}",
                innerException)
        {
        }

        public EventHandlingException(string topic, string message, Exception innerException)
            : base($"{message} Topic: '{topic}'", innerException)
        {
        }
    }
}