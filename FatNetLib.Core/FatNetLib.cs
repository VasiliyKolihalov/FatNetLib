using Kolyhalov.FatNetLib.Core.Subscribers;

namespace Kolyhalov.FatNetLib.Core
{
    public class FatNetLib
    {
        public ICourier Courier { get; }

        private readonly INetEventListener _netEventListener;

        public FatNetLib(ICourier courier, INetEventListener netEventListener)
        {
            Courier = courier;
            _netEventListener = netEventListener;
        }

        public void Run()
        {
            _netEventListener.Run();
        }

        public void Stop()
        {
            _netEventListener.Stop();
        }
    }
}
