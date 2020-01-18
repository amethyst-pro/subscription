using Amethyst.Subscription.Configurations;
using Microsoft.Extensions.Logging;

namespace Amethyst.Subscription.Handling
{
    public sealed class MessageHandlerFactory : IMessageHandlerFactory
    {
        private readonly IEventHandlerScopeFactory _scopeFactory;
        private readonly ILoggerFactory _loggerFactory;

        public MessageHandlerFactory(IEventHandlerScopeFactory scopeFactory, ILoggerFactory loggerFactory)
        {
            _scopeFactory = scopeFactory;
            _loggerFactory = loggerFactory;
        }

        public IMessageHandler Create(HandlerConfiguration config)
        {
            IMessageHandler handler = new MessageHandler(_scopeFactory, config.RunHandlersInParallel);

            handler = config.RetryPolicy != null
                ? new RetryingMessageHandler(handler, config.RetryPolicy)
                : new RetryingMessageHandler(
                    handler,
                    _loggerFactory,
                    config.RetryAttempts);

            if (config.IsLoggingEnabled)
                handler = new LoggingMessageHandler(_loggerFactory, handler);

            return handler;
        }
    }
}