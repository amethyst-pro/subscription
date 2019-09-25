namespace Amethyst.Subscription.Tests.Fakes
{
    public readonly struct CustomStruct
    {
        public CustomStruct(int x, int y)
        {
            X = x;
            Y = y;
        }
        
        public int X { get; }

        public int Y { get; }
    }
}