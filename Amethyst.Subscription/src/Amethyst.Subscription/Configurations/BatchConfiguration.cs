using System;

namespace Amethyst.Subscription.Configurations
{
    public sealed class BatchConfiguration
    {
        /// <summary>
        /// Defaults:
        /// maxBatchCount = 1000,
        /// batchTriggerTimeout = 60s,
        /// handlingStrategy = OrderedWithinKey. 
        /// </summary>
        /// <param name="maxBatchCount"></param>
        public BatchConfiguration()
            : this(TimeSpan.FromSeconds(60))
        {
        }

        public BatchConfiguration(
            TimeSpan batchTriggerTimeout,
            int maxBatchCount = 1000,
            BatchHandlingStrategy handlingStrategy = BatchHandlingStrategy.OrderedWithinKey)
        {
            MaxBatchCount = maxBatchCount;
            HandlingStrategy = handlingStrategy;
            BatchTriggerTimeout = batchTriggerTimeout;
        }

        public int MaxBatchCount { get; }

        public TimeSpan BatchTriggerTimeout { get; }

        public BatchHandlingStrategy HandlingStrategy { get; }
    }
}