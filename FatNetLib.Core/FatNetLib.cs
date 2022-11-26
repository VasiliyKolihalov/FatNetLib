using Kolyhalov.FatNetLib.Core.Couriers;
using Kolyhalov.FatNetLib.Core.Subscribers;

namespace Kolyhalov.FatNetLib.Core
{
    public class FatNetLib
    {
        public ICourier Courier { get; }

        public ServerCourier? ServerCourier => Courier as ServerCourier;

        public ClientCourier? ClientCourier => Courier as ClientCourier;

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
