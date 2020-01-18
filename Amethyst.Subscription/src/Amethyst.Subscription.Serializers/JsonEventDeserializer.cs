using System;
using Amethyst.Subscription.Abstractions;
using Utf8Json;
using Utf8Json.Resolvers;

namespace Amethyst.Subscription.Serializers
{
    public sealed class JsonEventDeserializer<T> : IEventDeserializer
    {
        private readonly IJsonFormatterResolver _resolver = StandardResolver.Default;

        public JsonEventDeserializer()
        {
        }

        public JsonEventDeserializer(IJsonFormatterResolver resolver)
            => _resolver = resolver;

        public IStreamEvent Deserialize(ReadOnlySpan<byte> message) =>
            new StreamEvent(JsonSerializer.Deserialize<T>(message.ToArray(), _resolver));
    }
}