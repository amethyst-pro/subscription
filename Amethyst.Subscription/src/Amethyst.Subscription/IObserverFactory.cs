using Amethyst.Subscription.Broker;

namespace Amethyst.Subscription
{
    public interface IObserverFactory
    {
        IObserver Create(IConsumer consumer);
    }
}