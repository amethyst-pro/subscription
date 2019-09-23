using Confluent.Kafka;

namespace Amethyst.Subscription.Broker
{
    public interface IConsumer
    {
        void Commit(params TopicPartitionOffset[] offsets);
        
        void Cancel();
    }
}