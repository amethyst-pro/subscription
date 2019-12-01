using System;
using Amethyst.Subscription.Broker;
using Amethyst.Subscription.Configurations;
using Amethyst.Subscription.Observing;
using Amethyst.Subscription.Observing.Batch;

namespace Amethyst.Subscription.Hosting
{
    public sealed class ObserverFactory : IObserverFactory
    {
        private readonly SubscriptionConfiguration _configuration;
        private readonly IMessageHandlerFactory _messageHandlerFactory;

        public ObserverFactory(
            SubscriptionConfiguration configuration,
            IMessageHandlerFactory messageHandlerFactory)
        {
            _configuration = configuration;
            _messageHandlerFactory = messageHandlerFactory;
        }

        public IObserver Create(IConsumer consumer)
        {
            var handler = _messageHandlerFactory.Create(_configuration.HandlerConfig);

            if (_configuration.BatchProcessingRequired)
            {
                return new BatchEventObserver(
                    _configuration.BatchConfiguration,
                    GetBatchHandler(),
                    consumer,
                    _configuration.SkipUnknownEvents);
            }

            return new EventObserver(handler, consumer, _configuration.SkipUnknownEvents);

            IBatchHandler GetBatchHandler()
            {
                var singleTypeHandler = new SingleTypeBatchHandler(handler);
                switch (_configuration.BatchConfiguration.HandlingStrategy)
                {
                    case BatchHandlingStrategy.SingleType:
                        return singleTypeHandler;
                    case BatchHandlingStrategy.OrderedWithinKey:
                        return new OrderedWithinKeyBatchHandler(singleTypeHandler);
                    case BatchHandlingStrategy.OrderedWithinType:
                        return new OrderedWithinTypeBatchHandler(singleTypeHandler);

                    default:
                        throw new InvalidOperationException(
                            $"Unknown handling strategy: {_configuration.BatchConfiguration.HandlingStrategy}");
                }
            }
        }
    }
}