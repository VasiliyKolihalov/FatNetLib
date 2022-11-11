using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Endpoints;

namespace Kolyhalov.FatNetLib.Core.Initializers
{
    public class EndpointsBody
    {
        public IList<Endpoint> Endpoints { get; set; } = null!;
    }
}
