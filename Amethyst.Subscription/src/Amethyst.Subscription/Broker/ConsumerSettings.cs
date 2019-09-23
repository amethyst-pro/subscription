using System;
using Confluent.Kafka;

namespace Amethyst.Subscription.Broker
{
    public sealed class ConsumerSettings
    {
        private const int DefaultMaxMessageBytes = 5 * 1024 * 1024;
        
        public ConsumerSettings()
        {
            Config = new ConsumerConfig
            {
                EnableAutoCommit = false,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                MessageMaxBytes = DefaultMaxMessageBytes
            };
        }

        public ConsumerSettings(string brokers, string groupId)
            : this()
        {
            Config.BootstrapServers = brokers;
            Config.GroupId = groupId;
        }

        private ConsumerSettings(ConsumerSettings other)
        {
            Config = other.Config;
            Topic = other.Topic;
        }

        public ConsumerConfig Config { get; }

        public string Topic { get; }

        public void SetLongRunning(TimeSpan? maxHandlingTime = default)
        {
            Config.MaxPollIntervalMs = (int)(maxHandlingTime ?? TimeSpan.FromMinutes(15)).TotalMilliseconds;
            Config.SessionTimeoutMs = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
        }

        public ConsumerSettings ToLongRunning(TimeSpan? maxHandlingTime = default)
        {
            var clone = new ConsumerSettings(this);
            clone.SetLongRunning(maxHandlingTime);

            return clone;
        }
    }
}