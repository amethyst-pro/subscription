using Amethyst.Subscription.Broker;
using Amethyst.Subscription.Observing;

namespace Amethyst.Subscription.Hosting
{
    public sealed class ObserverFactory : IObserverFactory
    {
        private readonly SubscriptionConfiguration _configuration;
        private readonly IEventHandlerFactory _eventHandlerFactory;

        public ObserverFactory(SubscriptionConfiguration configuration, IEventHandlerFactory eventHandlerFactory)
        {
            _configuration = configuration;
            _eventHandlerFactory = eventHandlerFactory;
        }

        public IObserver Create(IConsumer consumer)
        {
            var handler = _eventHandlerFactory.Create(_configuration.HandlerConfig);

            if (_configuration.BatchProcessingRequired)
                return new BatchEventObserver(
                    _configuration.BatchConfiguration,
                    handler,
                    consumer,
                    _configuration.SkipUnknownEvents);

            return new EventObserver(handler, consumer, _configuration.SkipUnknownEvents);
        }
    }
}