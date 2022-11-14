using System;
using System.Collections.Generic;

namespace Kolyhalov.FatNetLib.Core.Monitors
{
    public class ResponsePackageMonitorStorage : IResponsePackageMonitorStorage
    {
        // Todo: research, how to send values through wait-pulse in monitor
        // Todo: maybe try using monitor as a data container?
        public IDictionary<Guid, Package> ResponsePackages { get; } = new Dictionary<Guid, Package>();

        public IDictionary<Guid, object> MonitorsObjects { get; } = new Dictionary<Guid, object>();
    }
}
