using Amethyst.Subscription.Configurations;
using Microsoft.Extensions.Logging;

namespace Amethyst.Subscription.Handling
{
    public sealed class EventHandlerFactory : IEventHandlerFactory
    {
        private readonly IEventHandlerScopeFactory _scopeFactory;
        private readonly ILoggerFactory _loggerFactory;

        public EventHandlerFactory(IEventHandlerScopeFactory scopeFactory, ILoggerFactory loggerFactory)
        {
            _scopeFactory = scopeFactory;
            _loggerFactory = loggerFactory;
        }

        public IEventHandler Create(HandlerConfiguration config)
        {
            IEventHandler handler = new EventHandler(_scopeFactory, config.RunHandlersInParallel);

            handler = config.RetryPolicy != null
                ? new RetryingEventHandler(handler, config.RetryPolicy)
                : new RetryingEventHandler(
                    handler,
                    _loggerFactory,
                    config.RetryAttempts);

            if (config.IsLoggingEnabled)
                handler = new LoggingEventHandler(_loggerFactory, handler);

            return handler;
        }
    }
}