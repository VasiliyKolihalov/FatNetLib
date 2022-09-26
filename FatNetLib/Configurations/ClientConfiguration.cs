﻿using Kolyhalov.FatNetLib.Microtypes;

namespace Kolyhalov.FatNetLib.Configurations;

public class ClientConfiguration : Configuration
{
    public string Address { get; }

    public ClientConfiguration(string address,
        Port port,
        Frequency? framerate,
        TimeSpan? exchangeTimeout) : base(port, framerate, exchangeTimeout)
    {
        Address = address ?? throw new FatNetLibException("Address cannot be null");
    }
}