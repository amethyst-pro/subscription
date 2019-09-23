using System;
using Amethyst.Subscription.Abstractions;
using Amethyst.Subscription.Broker;
using Confluent.Kafka;


namespace Amethyst.Subscription
{
    public interface IEventContext
    {
        TopicPartitionOffset Offset { get; }

        Guid GetKey();

        object GetEvent();

        DeserializationStatus Status { get; }

        void Acknowledge(IConsumer consumer);
    }
}