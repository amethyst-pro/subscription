using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Amethyst.Subscription.Abstractions;
using Amethyst.Subscription.Broker;
using Amethyst.Subscription.Broker.Exceptions;
using Amethyst.Subscription.Configurations;
using Confluent.Kafka;

namespace Amethyst.Subscription.Observing
{
   public sealed class BatchEventObserver : IObserver, IDisposable
    {
        private readonly IEventHandler _handler;
        private readonly BatchConfiguration _config;
        private readonly BatchBlock<(TopicPartitionOffset, object)> _batchBlock;
        private readonly ActionBlock<IReadOnlyCollection<(TopicPartitionOffset, object)>> _actionBlock;
        private readonly ActionBlock<TimeSpan> _delayBatchBlock;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly IConsumer _consumer;
        private readonly bool _skipUnknown;

        public BatchEventObserver(
            BatchConfiguration config,
            IEventHandler handler,
            IConsumer consumer,
            bool skipUnknown = true)
        {
            _handler = handler;
            _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
            _skipUnknown = skipUnknown;
            _config = config;

            _batchBlock = new BatchBlock<(TopicPartitionOffset, object)>(config.MaxBatchCount,
                new GroupingDataflowBlockOptions
                {
                    CancellationToken = _cancellationTokenSource.Token
                });

            _actionBlock = new ActionBlock<IReadOnlyCollection<(TopicPartitionOffset, object)>>(
                Handle,
                new ExecutionDataflowBlockOptions
                {
                    CancellationToken = _cancellationTokenSource.Token
                });

            _batchBlock.LinkTo(_actionBlock, new DataflowLinkOptions {PropagateCompletion = true});

            _delayBatchBlock = new ActionBlock<TimeSpan>(DelayTriggerBatch,
                new ExecutionDataflowBlockOptions
                {
                    BoundedCapacity = 1,
                    CancellationToken = _cancellationTokenSource.Token
                });
        }

        public async Task OnEventAppeared<T>(T context, CancellationToken token) where T : IEventContext
        {
            if (_actionBlock.Completion.IsFaulted)
                throw new InvalidOperationException("Batch processing faulted.", _actionBlock.Completion.Exception);

            if (_consumer == null)
                throw new InvalidOperationException("Consumer is not set.");

            token.ThrowIfCancellationRequested();

            try
            {
                _batchBlock.Post((context.Offset, GetEvent()));
            }
            catch (InvalidEventException)
            {
                await Complete();
                throw;
            }

            _delayBatchBlock.Post(_config.BatchTriggerTimeout);

            async Task Complete()
            {
                _batchBlock.TriggerBatch();
                _batchBlock.Complete();

                await _batchBlock.Completion;

                _actionBlock.Complete();
                await _actionBlock.Completion;
            }

            object GetEvent()
            {
                return !_skipUnknown || context.Status == DeserializationStatus.Success
                    ? context.GetEvent()
                    : default;
            }
        }

        public void Dispose()
        {
            _batchBlock.Complete();
            _actionBlock.Complete();
            _delayBatchBlock.Complete();

            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        private async Task DelayTriggerBatch(TimeSpan delay)
        {
            await Task.Delay(delay, _cancellationTokenSource.Token);

            _batchBlock.TriggerBatch();
        }

        private async Task Handle(IReadOnlyCollection<(TopicPartitionOffset offset, object @event)> eventContexts)
        {
            if (eventContexts.Count == 0)
                return;

            var groupedByTypeEvents = eventContexts
                .Where(c => c.@event != null)
                .Select(c => c.@event)
                .GroupBy(e => e.GetType());

            try
            {
                foreach (var group in groupedByTypeEvents)
                {
                    var typedEvents = group.ToArray();

                    await HandleBatch((dynamic) typedEvents[0], typedEvents);
                }
            }
            catch (Exception ex)
            {
                _cancellationTokenSource.Cancel();
                _consumer.Cancel();

                throw new EventHandlingException(
                    eventContexts.First().offset.Topic,
                    "Batch events handling failed.",
                    ex);
            }

            _consumer.Commit(GetLatestOffsets(eventContexts));
        }

        private TopicPartitionOffset[] GetLatestOffsets(
            IReadOnlyCollection<(TopicPartitionOffset offset, object)> eventContexts)
        {
            var offsets = new Dictionary<TopicPartition, TopicPartitionOffset>(20);

            foreach (var (topicPartitionOffset, _) in eventContexts)
            {
                if (!offsets.TryGetValue(topicPartitionOffset.TopicPartition, out var previousOffset) ||
                    topicPartitionOffset.Offset > previousOffset.Offset)
                {
                    offsets[topicPartitionOffset.TopicPartition] = topicPartitionOffset;
                }
            }

            return offsets.Values.ToArray();
        }

        private Task HandleBatch<T>(T typedItem, IReadOnlyCollection<object> events)
        {
            return _handler.Handle<IReadOnlyCollection<T>>(events.Cast<T>().ToArray(), CancellationToken.None);
        }
    }
}