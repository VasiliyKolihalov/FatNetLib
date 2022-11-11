using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.Wrappers;

namespace Kolyhalov.FatNetLib
{
    public class ServerConnectionStarter : IConnectionStarter
    {
        private readonly INetManager _netManager;
        private readonly Port _port;

        public ServerConnectionStarter(INetManager netManager, Port port)
        {
            _netManager = netManager;
            _port = port;
        }

        public void StartConnection()
        {
            _netManager.Start(_port.Value);
        }
    }
}
