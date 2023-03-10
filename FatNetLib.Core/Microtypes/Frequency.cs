﻿using System;
using Kolyhalov.FatNetLib.Core.Exceptions;

namespace Kolyhalov.FatNetLib.Core.Microtypes
{
    // Unit of measurement is Hz or 1/seconds
    public class Frequency
    {
        public int Value { get; }

        public TimeSpan Period => TimeSpan.FromSeconds(1) / Value;

        public Frequency(int value)
        {
            if (value < 0)
                throw new FatNetLibException("Frequency cannot be below zero");

            Value = value;
        }
    }
}
