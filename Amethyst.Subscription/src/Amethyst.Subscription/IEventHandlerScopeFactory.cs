namespace Amethyst.Subscription
{
    public interface IEventHandlerScopeFactory
    {
        IEventHandlerScope BeginScope();
    }
}