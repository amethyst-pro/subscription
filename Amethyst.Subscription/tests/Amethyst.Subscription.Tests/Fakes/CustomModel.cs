using System.Runtime.Serialization;

namespace Amethyst.Subscription.Tests.Fakes
{
    [DataContract]
    public sealed class CustomModel
    {
        [DataMember(Name = "prop_a")]
        public long PropA { get; set; }

        [DataMember(Name = "prop_b")]
        public long PropB { get; set; }

        [DataMember]
        public int PropC { get; set; }
    }
}