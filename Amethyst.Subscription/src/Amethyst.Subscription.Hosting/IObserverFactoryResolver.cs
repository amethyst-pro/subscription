namespace Amethyst.Subscription.Hosting
{
    public interface IObserverFactoryResolver
    {
        IObserverFactory Resolve(SubscriptionConfiguration config);
    }
}