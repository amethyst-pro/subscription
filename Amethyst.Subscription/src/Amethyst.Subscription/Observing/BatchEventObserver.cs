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
        private readonly BatchBlock<Message> _batchBlock;
        private readonly ActionBlock<Message[]> _actionBlock;
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

            _batchBlock = new BatchBlock<Message>(config.MaxBatchCount,
                new GroupingDataflowBlockOptions
                {
                    CancellationToken = _cancellationTokenSource.Token,
                    BoundedCapacity = config.MaxBatchCount
                });

            _actionBlock = new ActionBlock<Message[]>(
                HandleBatch,
                new ExecutionDataflowBlockOptions
                {
                    CancellationToken = _cancellationTokenSource.Token
                });

            _batchBlock.LinkTo(_actionBlock, new DataflowLinkOptions { PropagateCompletion = true });

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
                await _batchBlock.SendAsync(
                    new Message(context.Offset, context.GetKey(), GetEvent()),
                    token);
            }
            catch (InvalidEventException)
            {
                await Complete();
                throw;
            }

            _delayBatchBlock.Post(_config.BatchTriggerTimeout);

            object GetEvent()
            {
                return !_skipUnknown || context.Status == DeserializationStatus.Success
                    ? context.GetEvent()
                    : default;
            }
        }

        public async Task Complete()
        {
            _batchBlock.TriggerBatch();
            _batchBlock.Complete();

            await _batchBlock.Completion;

            _actionBlock.Complete();
            await _actionBlock.Completion;
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

        private async Task HandleBatch(Message[] messages)
        {
            if (messages.Length == 0)
                return;

            var topic = messages[0].Offset.Topic;

            try
            {
                var latestOffsets = GetLatestOffsets(messages);

                await Handle(messages.Where(e => e.Event != null).ToArray());

                _consumer.Commit(latestOffsets);
            }
            catch (Exception ex)
            {
                _cancellationTokenSource.Cancel();
                _consumer.Cancel();

                throw new EventHandlingException(
                    topic,
                    "Batch events handling failed.",
                    ex);
            }
        }

        private async Task Handle(Message[] messages)
        {
            if (AllMessagesHasSameType())
            {
                await HandleTyped(
                    (dynamic)messages[0].Event,
                    messages.Select(m => m.Event));
            }
            else
            {
                var remainingMessages = new List<Message>(messages);

                while (remainingMessages.Count > 0)
                {
                    var generation = new Dictionary<Guid, object>(remainingMessages.Count / 2 + 1);
                    var nextGenerationMessages = new List<Message>(remainingMessages.Count / 2);

                    foreach (var message in remainingMessages)
                    {
                        if (!generation.TryAdd(message.Key, message.Event))
                            nextGenerationMessages.Add(message);
                    }

                    remainingMessages = nextGenerationMessages;

                    await HandleGeneration(generation.Values);
                }
            }

            bool AllMessagesHasSameType()
            {
                var sampleType = messages[^1].Event.GetType();

                return Array.TrueForAll(messages, m => m.Event.GetType() == sampleType);
            }
        }

        private async Task HandleGeneration(IEnumerable<object> events)
        {
            var groupedByTypeEvents = events
                .GroupBy(e => e.GetType());

            foreach (var group in groupedByTypeEvents)
            {
                var typedEvents = @group.ToArray();

                await HandleTyped((dynamic)typedEvents[0], typedEvents);
            }
        }

        private Task HandleTyped<T>(T sample, IEnumerable<object> events)
        {
            return _handler.Handle<IReadOnlyCollection<T>>(
                events.Cast<T>().ToArray(),
                CancellationToken.None);
        }

        private static TopicPartitionOffset[] GetLatestOffsets(Message[] messages)
        {
            var offsets = new Dictionary<TopicPartition, TopicPartitionOffset>(20);

            foreach (var message in messages)
            {
                if (!offsets.TryGetValue(message.Offset.TopicPartition, out var previousOffset) ||
                    message.Offset.Offset > previousOffset.Offset)
                {
                    offsets[message.Offset.TopicPartition] = message.Offset;
                }
            }

            return offsets.Values.ToArray();
        }

        private sealed class Message
        {
            public TopicPartitionOffset Offset { get; }
            public object Event { get; }
            public Guid Key { get; }

            public Message(TopicPartitionOffset offset, Guid key, object @event)
            {
                Offset = offset;
                Event = @event;
                Key = key;
            }
        }
    }
}