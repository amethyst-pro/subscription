using System;

namespace Amethyst.Subscription
{
    public interface IEventHandlerScopeFactory : IDisposable
    {
        IEventHandlerScope BeginScope();
    }
}