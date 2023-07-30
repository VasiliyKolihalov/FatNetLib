using System;
using System.Threading.Tasks;
using Kolyhalov.FatNetLib.Core.Models;

namespace Kolyhalov.FatNetLib.Core.Monitors
{
    public interface IResponsePackageMonitor
    {
        public Task<Package> WaitAsync(Guid exchangeId);

        public void Pulse(Package responsePackage);
    }
}
