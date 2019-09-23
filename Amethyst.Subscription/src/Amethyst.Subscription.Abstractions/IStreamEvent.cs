using System.Collections.Generic;

namespace Amethyst.Subscription.Abstractions
{
    public interface IStreamEvent
    {
        object Event { get; }

        DeserializationStatus Status { get; }

        IReadOnlyCollection<KeyValuePair<string, object>> Metadata { get; }
    }
}