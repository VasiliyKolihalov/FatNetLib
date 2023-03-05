using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core.Subscribers.Server
{
    public class ServerConnectionStarter : IConnectionStarter
    {
        private readonly INetManager _netManager;
        private readonly Configuration _configuration;

        public ServerConnectionStarter(INetManager netManager, Configuration configuration)
        {
            _netManager = netManager;
            _configuration = configuration;
        }

        public void StartConnection()
        {
            bool started = _netManager.Start(_configuration.Port!.Value);
            if (!started)
                throw new FatNetLibException("Can't start server");
        }
    }
}
