using System.Collections.Generic;
using Kolyhalov.FatNetLib.Endpoints;

namespace Kolyhalov.FatNetLib.Initializers
{
    public class EndpointsBody
    {
        public IList<Endpoint> Endpoints { get; set; } = null!;
    }
}
