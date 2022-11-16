﻿using System;
using Kolyhalov.FatNetLib.Core.Models;
using static System.AttributeTargets;

namespace Kolyhalov.FatNetLib.Core.Attributes
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
