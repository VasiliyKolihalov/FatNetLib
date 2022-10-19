﻿using Kolyhalov.FatNetLib.Microtypes;

namespace Kolyhalov.FatNetLib.Configurations;

public class ConfigurationOptions
{
    public Port Port { get; set; }
    public Frequency Framerate { get; set; }
    public TimeSpan ExchangeTimeout { get; set; }
}