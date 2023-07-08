using System;
using System.Threading.Tasks;
using Kolyhalov.FatNetLib.Core.Models;

namespace Kolyhalov.FatNetLib.Core.Monitors
{
    public interface IResponsePackageMonitor
    {
        public void WaitAsync(Guid exchangeId, TaskCompletionSource<Package> taskCompletionSource);

        public void Pulse(Package responsePackage);
    }
}
