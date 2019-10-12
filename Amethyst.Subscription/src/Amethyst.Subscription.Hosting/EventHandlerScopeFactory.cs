using System;
using System.Collections.Generic;
using System.Linq;
using Amethyst.Subscription.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Amethyst.Subscription.Hosting
{
    public sealed class EventHandlerScopeFactory : IEventHandlerScopeFactory
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public EventHandlerScopeFactory(IServiceScopeFactory scopeFactory) =>
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));

        public IEventHandlerScope BeginScope() => new EventHandlerScope(_scopeFactory.CreateScope());

        private sealed class EventHandlerScope : IEventHandlerScope
        {
            private readonly IServiceScope _scope;

            public EventHandlerScope(IServiceScope scope) => _scope = scope;

            public IReadOnlyList<IEventHandler<T>> Resolve<T>()
            {
                return _scope.ServiceProvider.GetServices<IEventHandler<T>>()
                    .ToArray();
            }

            public void Dispose() => _scope.Dispose();
        }
    }
}