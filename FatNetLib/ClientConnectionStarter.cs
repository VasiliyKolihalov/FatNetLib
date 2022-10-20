using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Wrappers;

namespace Kolyhalov.FatNetLib;

public class ClientConnectionStarter : IConnectionStarter
{
    private readonly INetManager _netManager;
    private readonly ClientConfiguration _configuration;
    private readonly string _protocolVersion;

    public ClientConnectionStarter(INetManager netManager,
        ClientConfiguration configuration,
        IProtocolVersionProvider protocolVersionProvider)
    {
        _netManager = netManager;
        _configuration = configuration;
        _protocolVersion = protocolVersionProvider.Get();
    }

    public void StartConnection()
    {
        _netManager.Start();
        _netManager.Connect(_configuration.Address,
            _configuration.Port.Value,
            key: _protocolVersion);
    }
}