using System.Threading;
using System.Threading.Tasks;
using Amethyst.Subscription.Abstractions;
using Amethyst.Subscription.Broker;

namespace Amethyst.Subscription.Observing
{
    public sealed class EventObserver : IObserver
    {
        private readonly IMessageHandler _handler;
        private readonly IConsumer _consumer;
        private readonly bool _skipUnknown;

        public EventObserver(
            IMessageHandler handler, 
            IConsumer consumer,
            bool skipUnknown = true)
        {
            _handler = handler;
            _consumer = consumer;
            _skipUnknown = skipUnknown;
        }

        public async Task OnEventAppeared<T>(T context, CancellationToken token)
            where T : IEventContext
        {
            if (!_skipUnknown || context.Status == DeserializationStatus.Success)
            {
                dynamic deserializedEvent = context.GetEvent();

                await _handler.HandleAsync(deserializedEvent, token);
            }

            context.Acknowledge(_consumer);
        }
    }
}