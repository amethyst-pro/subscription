using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Amethyst.Subscription.Abstractions;
using Amethyst.Subscription.Hosting.Comparers;
using Microsoft.Extensions.DependencyInjection;

namespace Amethyst.Subscription.Hosting.Extensions
{
    public sealed class EventHandlerScope : IEventHandlerScope
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public EventHandlerScope(IServiceScopeFactory scopeFactory)
            => _scopeFactory = scopeFactory;

        public IReadOnlyList<IEventHandler<T>> Resolve<T>()
        {
            using (var scope = _scopeFactory.CreateScope()){
                var resolved = scope.ServiceProvider.GetService<ReadOnlyCollection<IEventHandler<T>>>();
                    
                if (resolved.Count < 2)
                    return resolved;

                if (resolved.Count == 2 && !ReferenceEquals(resolved[0], resolved[1]))
                    return resolved;

                return resolved
                    .Distinct(ReferenceEqualityComparer<IEventHandler<T>>.Instance)
                    .ToArray();
            }
        }
    }
}