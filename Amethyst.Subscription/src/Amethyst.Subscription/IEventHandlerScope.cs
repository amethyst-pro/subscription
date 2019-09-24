using System;
using System.Collections.Generic;
using Amethyst.Subscription.Abstractions;

namespace Amethyst.Subscription
{
    public interface IEventHandlerScope : IDisposable
    {
        IReadOnlyList<IEventHandler<T>> Resolve<T>();
    }
}