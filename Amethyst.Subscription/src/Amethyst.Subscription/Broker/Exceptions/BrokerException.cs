using System;

namespace Amethyst.Subscription.Broker.Exceptions
{
    public sealed class BrokerException : Exception
    {
        public BrokerException(Exception innerException)
            : base(innerException.Message, innerException)
        {
        }
    }
}