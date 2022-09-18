using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.NetPeers;
using Kolyhalov.FatNetLib.ResponsePackageMonitors;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static Kolyhalov.FatNetLib.ExceptionUtils;

namespace Kolyhalov.FatNetLib;

public abstract class PackageListener
{
    protected readonly EventBasedNetListener Listener;
    protected readonly NetManager NetManager;
    private readonly IPackageHandler _packageHandler;
    protected readonly IList<INetPeer> ConnectedPeers;
    protected readonly IEndpointsStorage EndpointsStorage;
    private readonly IResponsePackageMonitor _responsePackageMonitor;
    protected readonly ILogger? Logger;
    private bool _isStop;

    protected PackageListener(EventBasedNetListener listener, 
        NetManager netManager, 
        IPackageHandler packageHandler,
        IList<INetPeer> connectedPeers, 
        IEndpointsStorage endpointsStorage,
        IResponsePackageMonitor responsePackageMonitor, 
        ILogger? logger)
    {
        Listener = listener;
        NetManager = netManager;
        _packageHandler = packageHandler;
        ConnectedPeers = connectedPeers;
        EndpointsStorage = endpointsStorage;
        _responsePackageMonitor = responsePackageMonitor;
        Logger = logger;
    }

    protected abstract Configuration Configuration { get; }

    public void Run()
    {
        StartListen();

        if (_isStop)
            throw new FatNetLibException("FatNetLib finished work");

        Listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            CatchExceptionsTo(Logger, () =>
            {
                string jsonPackage = dataReader.GetString();
                Package package;
                try
                {
                    package = JsonConvert.DeserializeObject<Package>(jsonPackage)
                              ?? throw new FatNetLibException("Deserialized package is null");
                }
                catch (Exception exception)
                {
                    throw new FatNetLibException("Failed to deserialize package", exception);
                }

                if (package.Route!.Contains("connection"))
                    return;

                if (package.IsResponse)
                {
                    _responsePackageMonitor.Pulse(package);
                }
                else
                {
                    _packageHandler.InvokeEndpoint(package, fromPeer.Id, deliveryMethod);
                }
            });

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
    

    //Todo: remove when we add connection endpoints
    protected void SendMessage(Package package, int peerId, DeliveryMethod deliveryMethod)
    {
        var peer = ConnectedPeers.Single(netPeer => netPeer.Id == peerId);
        string jsonPackage = JsonConvert.SerializeObject(package);
        var writer = new NetDataWriter();
        writer.Put(jsonPackage);
        peer.Send(writer, deliveryMethod);
    }
}