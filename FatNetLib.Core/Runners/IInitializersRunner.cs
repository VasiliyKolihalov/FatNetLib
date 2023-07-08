using System.Threading.Tasks;

namespace Kolyhalov.FatNetLib.Core.Runners
{
    public interface IInitializersRunner
    {
        public Task RunAsync();
    }
}
