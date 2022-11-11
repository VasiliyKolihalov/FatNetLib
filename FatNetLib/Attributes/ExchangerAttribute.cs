using System;
using static System.AttributeTargets;

namespace Kolyhalov.FatNetLib.Attributes
{
    [AttributeUsage(Method)]
    public class ExchangerAttribute : Attribute
    {
        public Reliability Reliability { get; }

        public ExchangerAttribute(Reliability reliability = Reliability.ReliableOrdered)
        {
            Reliability = reliability;
        }
    }
}
