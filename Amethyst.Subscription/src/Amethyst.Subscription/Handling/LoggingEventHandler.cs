using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Amethyst.Subscription.Handling
{
    public sealed class LoggingEventHandler : IEventHandler
    {
        private readonly IEventHandler _next;
        private readonly ILogger<LoggingEventHandler> _logger;

        public LoggingEventHandler(ILoggerFactory factory, IEventHandler next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = factory.CreateLogger<LoggingEventHandler>();
        }

        public Task Handle<T>(T @event, CancellationToken token)
        {
            _logger.LogInformation($"Event received: {@event.GetType().Name}.");

            return _next.Handle(@event, token);
        }
}