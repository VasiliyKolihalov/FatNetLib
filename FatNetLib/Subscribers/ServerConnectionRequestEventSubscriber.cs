using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Wrappers;
using Microsoft.Extensions.Logging;
using ILoggerProvider = Kolyhalov.FatNetLib.Loggers.ILoggerProvider;

namespace Kolyhalov.FatNetLib.Subscribers;

public class ServerConnectionRequestEventSubscriber : IConnectionRequestEventSubscriber
{
    private readonly ServerConfiguration _configuration;
    private readonly INetManager _netManager;
    private readonly string _protocolVersion;
    private readonly ILoggerProvider _loggerProvider;

    public ServerConnectionRequestEventSubscriber(ServerConfiguration configuration,
        INetManager netManager,
        IProtocolVersionProvider protocolVersionProvider,
        ILoggerProvider loggerProvider)
    {
        _configuration = configuration;
        _netManager = netManager;
        _protocolVersion = protocolVersionProvider.Get();
        _loggerProvider = loggerProvider;
    }

    public void Handle(IConnectionRequest request)
    {
        if (_netManager.ConnectedPeersCount >= _configuration.MaxPeers!.Value)
        {
            _loggerProvider.Logger?.LogWarning("Connection rejected: Max peers exceeded");
            request.Reject();
            return;
        }

        if (_protocolVersion != request.Data.GetString())
        {
            _loggerProvider.Logger?.LogWarning("Connection rejected: Protocol version mismatch");
            request.Reject();
            return;
        }

        request.Accept();
    }
}