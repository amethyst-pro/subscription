using System;

namespace Amethyst.Subscription.Configurations
{
    public sealed class BatchConfiguration
    {
        public BatchConfiguration()
            : this(TimeSpan.FromSeconds(60))
        {
        }

        public BatchConfiguration(
            TimeSpan batchTriggerTimeout,
            int maxBatchCount = 1000)
        {
            MaxBatchCount = maxBatchCount;
            BatchTriggerTimeout = batchTriggerTimeout;
        }
        
        public int MaxBatchCount { get; }

        public TimeSpan BatchTriggerTimeout { get; }
    }
}