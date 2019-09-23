using System;
using System.Collections.Generic;
using Amethyst.Subscription.Abstractions;

namespace Amethyst.Subscription.Serializers
{
    public sealed class StreamEvent : IStreamEvent
    {
        public StreamEvent(object @event)
            => Event = @event;

        public object Event { get; }
        
        public DeserializationStatus Status => DeserializationStatus.Success;

        public IReadOnlyCollection<KeyValuePair<string, object>> Metadata =>
            Array.Empty<KeyValuePair<string, object>>();
    }
}