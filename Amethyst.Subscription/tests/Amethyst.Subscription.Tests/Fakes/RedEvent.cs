using System;

namespace Amethyst.Subscription.Tests.Fakes
{
    public sealed class RedEvent
    {
        public RedEvent(
            int propA, 
            string propB, 
            Guid propC, 
            DateTime propD, 
            CustomStruct propE)
        {
            PropA = propA;
            PropB = propB;
            PropC = propC;
            PropD = propD;
            PropE = propE;
        }
        
        public int PropA { get; }
        
        public string PropB { get; }
        
        public Guid PropC { get; }
        
        public DateTime PropD { get; }
        
        public CustomStruct PropE { get; }
    }
}