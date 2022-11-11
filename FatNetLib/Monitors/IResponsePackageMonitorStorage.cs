using System;
using System.Collections.Generic;

namespace Kolyhalov.FatNetLib.Monitors
{
    public interface IResponsePackageMonitorStorage
    {
        public Dictionary<Guid, Package> ResponsePackages { get; }

        public Dictionary<Guid, object> MonitorsObjects { get; }
    }
}
