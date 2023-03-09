using System;

namespace Kolyhalov.FatNetLib.Core.Models
{
    public class EndpointRunFailedView
    {
        public Type Type { get; set; } = null!;

        public string? Message { get; set; }

        public EndpointRunFailedView? InnerException { get; set; }
    }
}
