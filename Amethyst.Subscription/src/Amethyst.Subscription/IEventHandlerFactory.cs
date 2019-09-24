using Amethyst.Subscription.Configurations;

namespace Amethyst.Subscription
{
    public interface IEventHandlerFactory
    {
        IEventHandler Create(HandlerConfiguration config);
    }
}