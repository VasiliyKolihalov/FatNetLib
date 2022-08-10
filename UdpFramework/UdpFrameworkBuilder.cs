using LiteNetLib;
using Microsoft.Extensions.Logging;

namespace Kolyhalov.UdpFramework;

public static class UdpFrameworkBuilder
{
    private static IEndpointsStorage EndpointStorage => new EndpointsStorage();
    private static IEndpointsInvoker EndpointsInvoker => new EndpointsInvoker();
    private static EventBasedNetListener EventBasedNetListener => new EventBasedNetListener();
    
    public static ServerUdpFramework BuildServer(ServerBuilderOptions builderOptions)
    {
        builderOptions.Validate();
        if (builderOptions.Logger == null)
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            builderOptions.Logger = loggerFactory.CreateLogger<UdpFramework>();
        }
      

        var serverConfiguration = new ServerConfiguration(
            builderOptions.Port.UdpPort,
            string.Empty, //Todo control version 
            builderOptions.Framerate.ServerFramerate, 
            builderOptions.MaxPeersCount);

        return new ServerUdpFramework(serverConfiguration,
            builderOptions.Logger, 
            EndpointStorage,
            EndpointsInvoker, 
            EventBasedNetListener);
    }

    public static ClientUdpFramework BuildClient(ClientBuilderOptions builderOptions)
    {
        builderOptions.Validate();
        if (builderOptions.Logger == null)
        {
            ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            builderOptions.Logger = loggerFactory.CreateLogger<UdpFramework>();
        }

        var clientConfiguration = new ClientConfiguration(
            builderOptions.Address,
            builderOptions.Port.UdpPort,
            string.Empty, //Todo control version 
            builderOptions.Framerate.ServerFramerate);

        return new ClientUdpFramework(clientConfiguration, 
            builderOptions.Logger, 
            EndpointStorage, 
            EndpointsInvoker,
            EventBasedNetListener);

    }
}

