using Kolyhalov.FatNetLib.Core.Models;

namespace Kolyhalov.FatNetLib.Core.Runners
{
    public interface IMiddlewaresRunner
    {
        public void Process(Package package);
    }
}
