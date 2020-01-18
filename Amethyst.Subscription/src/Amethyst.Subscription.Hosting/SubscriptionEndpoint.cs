using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amethyst.Subscription.Broker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Amethyst.Subscription.Hosting
{
    public sealed class SubscriptionEndpoint : BackgroundService
    {
        private readonly EndpointConfiguration _configuration;
        private readonly IMessageHandlerFactory _messageHandlerFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        public SubscriptionEndpoint(
            EndpointConfiguration configuration,
            IMessageHandlerFactory messageHandlerFactory,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _messageHandlerFactory =
                messageHandlerFactory ?? throw new ArgumentNullException(nameof(messageHandlerFactory));
            _serviceProvider = serviceProvider;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<SubscriptionEndpoint>();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var subscriptionTasks = _configuration.Subscriptions
                .SelectMany(c =>
                    Enumerable.Range(0, c.ConsumerInstances)
                        .Select(_ => RunConsuming(c, stoppingToken)))
                .ToArray();

            return Task.WhenAll(subscriptionTasks);
        }

        private async Task RunConsuming(SubscriptionConfiguration config, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation(
                        $"Subscription starting. Topic {config.Settings.Topic}. Group {config.Settings.Config.GroupId}");

                    using var consumer = CreateConsumer(config);
                    try
                    {
                        await consumer.ConsumeAsync(cancellationToken);
                    }
                    finally
                    {
                        consumer.Close();
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation($"Subscription stopped. Topic: {config.Settings.Topic}.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Subscription failed. Topic: {config.Settings.Topic}.");
                }
            }
        }

        private KafkaConsumer CreateConsumer(SubscriptionConfiguration config)
        {
            return new KafkaConsumer(
                new ObserverFactory(config, _messageHandlerFactory),
                config.Serializer,
                config.Settings,
                _loggerFactory);
        }
    }
}