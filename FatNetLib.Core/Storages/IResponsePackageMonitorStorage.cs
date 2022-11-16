using System;
using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Models;

namespace Kolyhalov.FatNetLib.Core.Storages
{
    public interface IResponsePackageMonitorStorage
    {
        public IDictionary<Guid, Package> ResponsePackages { get; }

        public IDictionary<Guid, object> MonitorsObjects { get; }
    }
}
