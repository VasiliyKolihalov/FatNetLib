using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.Wrappers;

namespace Kolyhalov.FatNetLib
{
    public class ClientConnectionStarter : IConnectionStarter
    {
        private readonly INetManager _netManager;
        private readonly string _address;
        private readonly Port _port;
        private readonly string _protocolVersion;

        public ClientConnectionStarter(
            INetManager netManager,
            string address,
            Port port,
            IProtocolVersionProvider protocolVersionProvider)
        {
            _netManager = netManager;
            _address = address;
            _port = port;
            _protocolVersion = protocolVersionProvider.Get();
        }

        public void StartConnection()
        {
            _netManager.Start();
            _netManager.Connect(_address, _port.Value, key: _protocolVersion);
        }
    }
}
