using System;
using Kolyhalov.FatNetLib.Core.Models;
using static System.AttributeTargets;

namespace Kolyhalov.FatNetLib.Core.Attributes
{
    [AttributeUsage(Method)]
    public class ConsumerAttribute : Attribute
    {
        public Reliability Reliability { get; }

        public ConsumerAttribute(Reliability reliability = Reliability.ReliableOrdered)
        {
            Reliability = reliability;
        }
    }
}
