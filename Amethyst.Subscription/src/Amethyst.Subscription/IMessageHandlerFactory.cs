using Amethyst.Subscription.Configurations;

namespace Amethyst.Subscription
{
    public interface IMessageHandlerFactory
    {
        IMessageHandler Create(HandlerConfiguration config);
    }
}