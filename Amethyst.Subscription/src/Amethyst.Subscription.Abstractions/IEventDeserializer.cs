using System;

namespace Amethyst.Subscription.Abstractions
{
    public interface IEventDeserializer
    {
        IStreamEvent Deserialize(ReadOnlySpan<byte> message);
    }
}