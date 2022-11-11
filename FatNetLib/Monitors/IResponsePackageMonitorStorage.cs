using System;
using System.Collections.Generic;

namespace Kolyhalov.FatNetLib.Monitors
{
    public interface IResponsePackageMonitorStorage
    {
        public IDictionary<Guid, Package> ResponsePackages { get; }

        public IDictionary<Guid, object> MonitorsObjects { get; }
    }
}
