using System.Collections.Generic;
using Amethyst.Subscription.Abstractions;

namespace Amethyst.Subscription
{
    public interface IEventHandlerScope
    {
        IReadOnlyList<IEventHandler<T>> Resolve<T>();
    }
}