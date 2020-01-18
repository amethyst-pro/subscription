using System;

namespace Amethyst.Subscription.Tests.Fakes
{
    public sealed class BlueEvent
    {
        public Guid Id { get; }

        public BlueEvent(Guid id)
        {
            Id = id;
        }
    }
}