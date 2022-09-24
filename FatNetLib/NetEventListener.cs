using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.NetPeers;
using LiteNetLib;
using Microsoft.Extensions.Logging;
using static Kolyhalov.FatNetLib.ExceptionUtils;
using NetPeer = Kolyhalov.FatNetLib.NetPeers.NetPeer;

namespace Kolyhalov.FatNetLib;

public abstract class NetEventListener
{
    protected readonly EventBasedNetListener Listener;
    private readonly INetworkReceiveEventHandler _receiverEventHandler;
    protected readonly NetManager NetManager;
    protected readonly IList<INetPeer> ConnectedPeers;
    protected readonly IEndpointsStorage EndpointsStorage;
    protected readonly ILogger? Logger;
    private bool _isStop;

    // Todo: ticket #26 remove this when connection endpoints are ready
    private readonly IMiddleware _serializationMiddleware = new SerializationMiddleware();
    protected readonly IMiddleware DeserializationMiddleware = new DeserializationMiddleware();
    protected readonly PackageSchema DefaultPackageSchema;

    protected NetEventListener(EventBasedNetListener listener,
        INetworkReceiveEventHandler receiverEventHandler,
        NetManager netManager,
        IList<INetPeer> connectedPeers,
        IEndpointsStorage endpointsStorage,
        ILogger? logger,
        PackageSchema defaultPackageSchema)
    {
        Listener = listener;
        _receiverEventHandler = receiverEventHandler;
        NetManager = netManager;
        ConnectedPeers = connectedPeers;
        EndpointsStorage = endpointsStorage;
        Logger = logger;
        DefaultPackageSchema = defaultPackageSchema;
    }

    protected abstract Configuration Configuration { get; }

    public void Run()
    {
        if (_isStop)
            throw new FatNetLibException("FatNetLib finished work");

        StartListen();

        Listener.NetworkReceiveEvent += (peer, reader, method) =>
        {
            CatchExceptionsTo(Logger, () => _receiverEventHandler.Handle(new NetPeer(peer), reader, method));
        };

        Task.Run(() =>
        {
            while (!_isStop)
            {
                CatchExceptionsTo(Logger, () =>
                    {
                        NetManager.PollEvents();
                        Thread.Sleep(Configuration.Framerate.Period);
                    },
                    exceptionMessage: "Polling events failed");
            }
        });
    }

    public void Stop()
    {
        _isStop = true;
        NetManager.Stop();
    }

    protected abstract void StartListen();

    // Todo: ticket #26 remove when we add connection endpoints
    protected void SendPackage(Package package, LiteNetLib.NetPeer netPeer, DeliveryMethod deliveryMethod)
    {
        _serializationMiddleware.Process(package);
        ConnectedPeers.Single(foundPeer => foundPeer.Id == netPeer.Id)
            .Send(package.Serialized!, deliveryMethod);
    }
}