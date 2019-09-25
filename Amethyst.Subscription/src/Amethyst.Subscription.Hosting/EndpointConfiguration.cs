using System;
using System.Collections.Generic;
using Amethyst.Subscription.Abstractions;
using Amethyst.Subscription.Broker;
using Amethyst.Subscription.Configurations;

namespace Amethyst.Subscription.Hosting
{
    public sealed class EndpointConfiguration
    {
        private readonly List<SubscriptionConfiguration> _subscriptions = new List<SubscriptionConfiguration>();

        public IReadOnlyCollection<SubscriptionConfiguration> Subscriptions => _subscriptions.AsReadOnly();

        public EndpointConfiguration AddSubscription(
            ConsumerSettings settings,
            IEventDeserializer serializer,
            HandlerConfiguration handlerConfig = default,
            bool skipUnknownEventTypes = true,
            int instances = 1)
        {
            var subscription = new SubscriptionConfiguration(
                settings,
                serializer,
                handlerConfig,
                skipUnknownEventTypes,
                instances);

            _subscriptions.Add(subscription);

            return this;
        }

        public EndpointConfiguration AddBatchSubscription(
            ConsumerSettings settings,
            BatchConfiguration batchConfig,
            IEventDeserializer serializer,
            HandlerConfiguration handlerConfig = default,
            bool skipUnknownEventTypes = true,
            int instances = 1,
            bool isLongRunning = true)
        {
            var subscription = new SubscriptionConfiguration(
                isLongRunning ? settings.ToLongRunning() : settings,
                batchConfig,
                serializer,
                handlerConfig,
                skipUnknownEventTypes,
                instances);

            _subscriptions.Add(subscription);

            return this;
        }

        public EndpointConfiguration AddSubscription(SubscriptionConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            _subscriptions.Add(config);

            return this;
        }
    }
}