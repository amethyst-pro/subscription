using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Amethyst.Subscription.Handling
{
    public sealed class LoggingMessageHandler : IMessageHandler
    {
        private readonly IMessageHandler _next;
        private readonly ILogger<LoggingMessageHandler> _logger;

        public LoggingMessageHandler(ILoggerFactory factory, IMessageHandler next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = factory.CreateLogger<LoggingMessageHandler>();
        }

        public Task HandleAsync<T>(T message, CancellationToken token)
        {
            _logger.LogInformation($"Event received: {message.GetType().Name}.");

            return _next.HandleAsync(message, token);
        }
    }
}