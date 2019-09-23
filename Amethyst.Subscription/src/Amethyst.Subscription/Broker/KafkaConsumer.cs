using System;
using System.Threading;
using System.Threading.Tasks;
using Amethyst.Subscription.Abstractions;
using Amethyst.Subscription.Broker.Exceptions;
using Amethyst.Subscription.Observing;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Amethyst.Subscription.Broker
{
    public sealed class KafkaConsumer : IDisposable
    {
        private static readonly IDeserializer<Guid> KeyDeserializer = new KeyGuidDeserializer();

        private readonly IObserverFactory _observerFactory;
        private readonly ILogger<KafkaConsumer> _logger;
        private readonly IConsumer<Guid, IStreamEvent> _consumer;

        public KafkaConsumer(
            IObserverFactory observerFactory,
            IEventDeserializer deserializer,
            ConsumerSettings settings,
            ILoggerFactory loggerFactory)
        {
            if (deserializer == null) throw new ArgumentNullException(nameof(deserializer));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            _observerFactory = observerFactory ?? throw new ArgumentNullException(nameof(observerFactory));

            _logger = loggerFactory.CreateLogger<KafkaConsumer>();


            if (string.IsNullOrWhiteSpace(settings.Config.BootstrapServers))
                throw new InvalidOperationException("Brokers not specified.");

            if (string.IsNullOrEmpty(settings.Topic))
                throw new InvalidOperationException("Topics not specified.");

            if (string.IsNullOrEmpty(settings.Config.GroupId))
                throw new InvalidOperationException("Group Id not specified.");

            _consumer = new ConsumerBuilder<Guid, IStreamEvent>(settings.Config)
                .SetKeyDeserializer(KeyDeserializer)
                .SetValueDeserializer(new ValueObjectDeserializer(deserializer))
                .SetErrorHandler((_, e) => _logger.LogError(
                    $"KafkaConsumer internal error: Topic: {settings.Topic}, {e.Reason}, Fatal={e.IsFatal}," +
                    $" IsLocal= {e.IsLocalError}, IsBroker={e.IsBrokerError}"))
                .Build();

            _consumer.Subscribe(settings.Topic);
        }

        public void Close() => _consumer.Close();

        public void Dispose() => _consumer.Dispose();

        public async Task ConsumeAsync(CancellationToken stoppingToken)
        {
            using (var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken))
            {
                var consumer = new ConsumerAdapter(tokenSource, _consumer);
                var observer = _observerFactory.Create(consumer);

                while (!tokenSource.IsCancellationRequested)
                {
                    var result = await Consume(tokenSource.Token);

                    await Observe(result, observer, tokenSource.Token);
                }
            }

            async Task<ConsumeResult<Guid, IStreamEvent>> Consume(CancellationToken token)
            {
                ConsumeResult<Guid, IStreamEvent> result;

                try
                {
                    result = await Task.Run(() => _consumer.Consume(token), token);
                }
                catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.Local_ValueDeserialization)
                {
                    //FIXME: handle serialization exception and park
                    LogError(ex, "Serialization exception");
                    throw;
                }

                return result;
            }
        }

        private async Task Observe(
            ConsumeResult<Guid, IStreamEvent> result,
            IObserver observer,
            CancellationToken token)
        {
            var context = new EventContext(result);

            var metadata = context.GetMetadata();

            using (metadata.Count > 0 ? _logger.BeginScope(metadata) : null)
            {
                try
                {
                    await observer.OnEventAppeared(context, token);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Event handling failed");

                    throw new EventHandlingException(context.Offset, "Event handling failed", ex);
                }
            }
        }

        private void LogError(Exception ex, string message)
        {
            var innerEx = ex.InnerException;

            var level = 0;
            while (innerEx != null)
            {
                _logger.LogError(innerEx, $"Inner-{++level}: {message} : {innerEx.Message}");

                innerEx = innerEx.InnerException;
            }

            _logger.LogError(ex, message + ":" + ex.Message);
        }
    }
}