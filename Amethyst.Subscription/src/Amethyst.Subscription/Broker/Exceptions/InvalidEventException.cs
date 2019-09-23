using System;
using Confluent.Kafka;

namespace Amethyst.Subscription.Broker.Exceptions
{
    public class InvalidEventException : EventHandlingException
    {
        public InvalidEventException(TopicPartitionOffset context, string message, Exception innerException)
            : base(context, message, innerException)
        {
        }

        public InvalidEventException(string topic, string message, Exception innerException)
            : base(topic, message, innerException)
        {
        }
    }
}