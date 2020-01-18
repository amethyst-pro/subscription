namespace Amethyst.Subscription.Configurations
{
    public enum BatchHandlingStrategy
    {
        /// <summary>
        /// All messages of batches have the same type. The most efficient strategy.
        /// </summary>
        SingleType,

        /// <summary>
        /// Total order of messages is preserved. The most inefficient strategy.
        /// </summary>
        OrderedWithinKey,

        /// <summary>
        /// Order of messages is preserved within the same type but not within the same key.
        /// </summary>
        OrderedWithinType
    }
}