﻿using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Wrappers;
using Microsoft.Extensions.Logging;

namespace Kolyhalov.FatNetLib.Subscribers;

public class ServerConnectionRequestEventSubscriber : IConnectionRequestEventSubscriber
{
    private readonly ServerConfiguration _configuration;
    private readonly INetManager _netManager;
    private readonly string _protocolVersion;
    private readonly ILogger? _logger;

    public ServerConnectionRequestEventSubscriber(ServerConfiguration configuration, 
        INetManager netManager,
        IProtocolVersionProvider protocolVersionProvider,
        ILogger? logger)
    {
        _configuration = configuration;
        _netManager = netManager;
        _protocolVersion = protocolVersionProvider.Get();
        _logger = logger;
    }

    public void Handle(IConnectionRequest request)
    {
        if (_netManager.ConnectedPeersCount >= _configuration.MaxPeers.Value)
        {
            _logger?.LogWarning("Connection rejected: Max peers exceeded");
            request.Reject();
            return;
        }

        if (_protocolVersion != request.Data.GetString())
        {
            _logger?.LogWarning("Connection rejected: Protocol version mismatch");
            request.Reject();
            return;
        }

        request.Accept();
    }
}