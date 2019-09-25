namespace Amethyst.Subscription.Hosting
{
    public sealed class ObserverFactoryResolver : IObserverFactoryResolver
    {
        private readonly IEventHandlerFactory _eventHandlerFactory;

        public ObserverFactoryResolver(IEventHandlerFactory eventHandlerFactory)
        {
            _eventHandlerFactory = eventHandlerFactory;
        }

        public IObserverFactory Resolve(SubscriptionConfiguration config) =>
            new ObserverFactory(config, _eventHandlerFactory);
    }
}