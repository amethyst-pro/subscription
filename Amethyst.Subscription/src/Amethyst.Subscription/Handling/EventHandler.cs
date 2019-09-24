using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amethyst.Subscription.Abstractions;

namespace Amethyst.Subscription.Handling
{
    public sealed class EventHandler : IEventHandler
    {
        private readonly IEventHandlerScopeFactory _scopeFactory;
        private readonly bool _executeInParallel;

        public EventHandler(IEventHandlerScopeFactory scopeFactory, bool executeInParallel)
        {
            _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
            _executeInParallel = executeInParallel;
        }

        public async Task Handle<T>(T @event, CancellationToken token)
        {
            using (var scope = _scopeFactory.BeginScope())
            {
                var handlers = scope.Resolve<T>();

                if (handlers.Count == 0)
                    return;

                if (handlers.Count == 1)
                    await handlers[0].HandleAsync(@event);

                else if (_executeInParallel)
                    await ExecuteInParallel(@event, handlers);

                else
                    await ExecuteSequentially(@event, token, handlers);
            }
        }

        private static async Task ExecuteSequentially<T>(
            T @event, 
            CancellationToken token,
            IReadOnlyCollection<IEventHandler<T>> handlers)
        {
            foreach (var handler in handlers)
            {
                token.ThrowIfCancellationRequested();

                await handler.HandleAsync(@event);
            }
        }

        private static Task ExecuteInParallel<T>(
            T @event, 
            IReadOnlyCollection<IEventHandler<T>> handlers)
        {
            var tasks = new List<Task>(handlers.Count);
            
            tasks.AddRange(handlers.Select(eventHandler => eventHandler.HandleAsync(@event)));

            return Task.WhenAll(tasks);
        }
    }
}