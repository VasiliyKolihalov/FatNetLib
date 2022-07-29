using System.Reflection;
using Kolyhalov.UdpFramework.Attributes;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Kolyhalov.UdpFramework;

public abstract class UdpFramework
{
    protected readonly ILogger Logger;
    private IEndpointsHandler _endpointsHandler;

    protected readonly EventBasedNetListener Listener = new();
    protected readonly NetManager NetManager;

    protected readonly Dictionary<int, List<Endpoint>> RemoteEndpoints = new();
    protected readonly List<LocalEndpoint> LocalEndpoints = new();

    protected readonly List<INetPeerShell> ConnectedPeers = new();

    private bool _isStop;

    protected UdpFramework(ILogger logger, IEndpointsHandler endpointsHandler)
    {
        Logger = logger;
        _endpointsHandler = endpointsHandler;

        NetManager = new NetManager(Listener);
    }

    public const BindingFlags EndpointSearch = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public;

    public delegate void ReceiverDelegate(Package package);

    public delegate Package ExchangerDelegate(Package package);

    public void AddController(IController controller)
    {
        Type type = controller.GetType();
        object[] controllerAttributes = type.GetCustomAttributes(false);
        string mainPath = "";
        foreach (var attribute in controllerAttributes)
        {
            if (attribute is not Route route) continue;
            string path = route.Path;
            if (string.IsNullOrEmpty(path))
                throw new UdpFrameworkException($"{type.FullName} path is null or empty");
            mainPath = path;
        }

        foreach (var method in type.GetMethods(EndpointSearch))
        {
            object[] methodAttributes = method.GetCustomAttributes(true);
            string? methodPath = null;
            EndpointType? endpointType = null;
            DeliveryMethod? deliveryMethod = null;
            foreach (var attribute in methodAttributes)
            {
                switch (attribute)
                {
                    case Route route:
                        string path = route.Path;
                        if (string.IsNullOrEmpty(path))
                            throw new UdpFrameworkException($"{method.Name} path in {type.Name} is null or empty");
                        methodPath = path;
                        break;

                    case Receiver receiver:
                        endpointType = EndpointType.Receiver;
                        deliveryMethod = receiver.DeliveryMethod;
                        break;

                    case Exchanger exchanger:
                        if (method.ReturnType != typeof(Package))
                            throw new UdpFrameworkException(
                                $"Return type of a {method.Name} in a {type.Name} must be package");

                        endpointType = EndpointType.Exchanger;
                        deliveryMethod = exchanger.DeliveryMethod;
                        break;
                }
            }

            if (methodPath == null)
                throw new UdpFrameworkException($"{method.Name} in {type.Name} does not have route attribute");

            if (endpointType == null)
                throw new UdpFrameworkException($"{method.Name} in {type.Name} does not have endpoint type attribute");

            string fullPath = mainPath + "/" + methodPath;
            if (LocalEndpoints.Select(x => x.EndpointData.Path).Contains(fullPath))
                throw new UdpFrameworkException($"Endpoint with this path : {fullPath} already registered");

            LocalEndpoints.Add(new LocalEndpoint(new Endpoint(fullPath, endpointType.Value, deliveryMethod!.Value),
                controller, method));
        }
    }

    public void AddReceiver(string route, DeliveryMethod deliveryMethod, ReceiverDelegate receiverDelegate)
    {
        var endpoint = new LocalEndpoint(new Endpoint(route, EndpointType.Receiver, deliveryMethod), controller: null,
            receiverDelegate?.Method!);

        ValidateEndpoint(endpoint);

        LocalEndpoints.Add(endpoint);
    }

    public void AddExchanger(string route, DeliveryMethod deliveryMethod, ExchangerDelegate exchangerDelegate)
    {
        var endpoint = new LocalEndpoint(new Endpoint(route, EndpointType.Exchanger, deliveryMethod), controller: null,
            exchangerDelegate?.Method!);

        ValidateEndpoint(endpoint);

        LocalEndpoints.Add(endpoint);
    }

    public abstract void Run();

    protected void StartListen(int framerate)
    {
        if (_isStop)
            throw new UdpFrameworkException("UdpFramework finished work");

        Listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
        {
            string jsonPackage = dataReader.GetString();
            Package package = JsonConvert.DeserializeObject<Package>(jsonPackage)!;
            if (package.Route.Contains("connection"))
                return;

            InvokeEndpoint(package, new NetPeerShell(fromPeer), deliveryMethod);
        };

        Listener.PeerConnectedEvent += peer => ConnectedPeers.Add(new NetPeerShell(peer));

        Task.Run(() =>
        {
            while (!_isStop)
            {
                NetManager.PollEvents();
                Thread.Sleep(TimeSpan.FromSeconds(1) / framerate);
            }
        });
    }

    public void Stop()
    {
        _isStop = true;
        NetManager.Stop();
    }

    public Package? SendPackage(Package package, int receiverId)
    {
        INetPeerShell receiver = ConnectedPeers.FirstOrDefault(peer => peer.Id == receiverId) ??
                                 throw new UdpFrameworkException("Receiver not found");

        Endpoint endpoint = RemoteEndpoints[receiver.Id].FirstOrDefault(endpoint => endpoint.Path == package.Route) ??
                            throw new UdpFrameworkException("Endpoint not found");

        DeliveryMethod deliveryMethod = endpoint.DeliveryMethod;

        SendMessage(package, receiver, deliveryMethod);

        if (endpoint.EndpointType == EndpointType.Receiver)
            return null;

        Package? responsePackage = null;
        throw new NotImplementedException();
        return responsePackage;
    }

    private void InvokeEndpoint(Package package, INetPeerShell peer, DeliveryMethod deliveryMethod)
    {
        LocalEndpoint? endpoint =
            LocalEndpoints.FirstOrDefault(endpoint => endpoint.EndpointData.Path == package.Route);
        if (endpoint == null)
        {
            Logger.LogError($"Request from {peer.Id} pointed to a non-existent endpoint. Route: {package.Route}");
            return;
        }

        if (endpoint.EndpointData.DeliveryMethod != deliveryMethod)
        {
            Logger.LogError($"Request from {peer.Id} came with the wrong type of delivery");
            return;
        }

        Package? responsePackage = _endpointsHandler.HandleEndpoint(endpoint, package);
        
        if(responsePackage != null)
            SendMessage(responsePackage, peer, deliveryMethod);            
        
    }

    protected void SendMessage(Package package, INetPeerShell peer, DeliveryMethod deliveryMethod)
    {
        string jsonPackage = JsonConvert.SerializeObject(package);
        NetDataWriter writer = new NetDataWriter();
        writer.Put(jsonPackage);
        peer.Send(writer, deliveryMethod);
    }

    private void ValidateEndpoint(LocalEndpoint endpoint)
    {
        if (string.IsNullOrEmpty(endpoint.EndpointData.Path))
            throw new UdpFrameworkException("Route is null or empty");

        if (endpoint.Method == null)
            throw new UdpFrameworkException("Method is null is null");

        if (LocalEndpoints.Select(x => x.EndpointData.Path).Contains(endpoint.EndpointData.Path))
            throw new UdpFrameworkException(
                $"Endpoint with this path : {endpoint.EndpointData.Path} already registered");
    }
}