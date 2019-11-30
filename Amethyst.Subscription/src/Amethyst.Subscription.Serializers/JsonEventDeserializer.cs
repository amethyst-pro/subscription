using System;
using System.Text.Json;
using Amethyst.Subscription.Abstractions;

namespace Amethyst.Subscription.Serializers
{
    public sealed class JsonEventDeserializer<T> : IEventDeserializer
    {
        private readonly JsonSerializerOptions _options;

        public JsonEventDeserializer()
        {
            _options = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true
            };
        }

        public JsonEventDeserializer(JsonSerializerOptions options)
            => _options = options;

        public IStreamEvent Deserialize(ReadOnlySpan<byte> message) =>
            new StreamEvent(JsonSerializer.Deserialize<T>(message, _options));
    }
}