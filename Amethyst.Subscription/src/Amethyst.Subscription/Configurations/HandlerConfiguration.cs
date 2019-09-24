using Polly;

namespace Amethyst.Subscription.Configurations
{
    public sealed class HandlerConfiguration
    {
        public HandlerConfiguration(
            bool isLoggingEnabled = false,
            int retryAttempts = 10,
            IAsyncPolicy retryPolicy = default,
            bool runHandlersInParallel = false)
        {
            IsLoggingEnabled = isLoggingEnabled;
            RetryAttempts = retryAttempts;
            RetryPolicy = retryPolicy;
            RunHandlersInParallel = runHandlersInParallel;
        }
        
        public bool IsLoggingEnabled { get; }

        public bool RunHandlersInParallel { get; }

        public int RetryAttempts { get; }

        public IAsyncPolicy RetryPolicy { get; }
    }
}