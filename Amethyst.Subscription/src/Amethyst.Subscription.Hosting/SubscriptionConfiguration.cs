using System;
using Amethyst.Subscription.Abstractions;
using Amethyst.Subscription.Broker;
using Amethyst.Subscription.Configurations;

namespace Amethyst.Subscription.Hosting
{
    public sealed class SubscriptionConfiguration
    {
        public SubscriptionConfiguration(
            ConsumerSettings settings,
            IEventDeserializer serializer,
            HandlerConfiguration handlerConfig = default,
            bool skipUnknownEvents = true,
            int consumerInstances = 1)
        {
            if(consumerInstances < 1)
                throw new ArgumentOutOfRangeException(
                    nameof(consumerInstances), 
                    consumerInstances, 
                    "Instances should be >= 1.");

            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            SkipUnknownEvents = skipUnknownEvents;
            ConsumerInstances = consumerInstances;
            HandlerConfig = handlerConfig ?? new HandlerConfiguration();
        }

        public SubscriptionConfiguration(
            ConsumerSettings settings,
            BatchConfiguration batchConfiguration,
            IEventDeserializer serializer,
            HandlerConfiguration handlerConfig = default,
            bool skipUnknownEvents = true,
            int consumerInstances = 1)
            : this(settings, serializer, handlerConfig, skipUnknownEvents, consumerInstances)
        {
            BatchConfiguration = batchConfiguration;
            BatchProcessingRequired = true;
        }

        public int ConsumerInstances { get; }

        public bool BatchProcessingRequired { get; }

        public bool SkipUnknownEvents { get; }

        public ConsumerSettings Settings { get; }

        public IEventDeserializer Serializer { get; }

        public HandlerConfiguration HandlerConfig { get; }

        public BatchConfiguration BatchConfiguration { get; }
    }
}