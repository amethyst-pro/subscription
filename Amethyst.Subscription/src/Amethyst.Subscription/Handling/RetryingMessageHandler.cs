using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Polly;

namespace Amethyst.Subscription.Handling
{
    public sealed class RetryingMessageHandler : IMessageHandler
    {
        private readonly IMessageHandler _handler;
        private readonly IAsyncPolicy _policy;

        public RetryingMessageHandler(IMessageHandler handler, IAsyncPolicy policy)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            _policy = policy ?? throw new ArgumentNullException(nameof(policy));
        }

        public RetryingMessageHandler(IMessageHandler handler, ILoggerFactory loggerFactory,
            int firstLevelRetryAttemptsCount)
        {
            const int longRetryDelayMinutes = 10;
            const int longRetryDurationMinutes = 3 * 60;

            _handler = handler ?? throw new ArgumentNullException(nameof(handler));

            var logger = loggerFactory.CreateLogger<RetryingMessageHandler>();
            _policy = Policy.Handle<Exception>()
                .WaitAndRetryAsync(
                    longRetryDurationMinutes / longRetryDelayMinutes + firstLevelRetryAttemptsCount,
                    retryAttempt =>
                        retryAttempt <= firstLevelRetryAttemptsCount
                            ? TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                            : TimeSpan.FromMinutes(longRetryDelayMinutes),
                    (ex, timeout, attempt, _) => logger.LogError(ex,
                        $"Event processing failed and will be retried. Attempt = {attempt}, timeout = {timeout}."));
        }

        public Task HandleAsync<T>(T message, CancellationToken token)
        {
            return _policy.ExecuteAsync(
                ct => _handler.HandleAsync(message, ct),
                token);
        }
    }
}