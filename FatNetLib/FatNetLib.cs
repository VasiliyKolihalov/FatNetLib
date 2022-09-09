using System.Linq.Expressions;
using System.Reflection;
using Kolyhalov.FatNetLib.Attributes;
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

// todo: add debug logs
// todo: refactor this huge class
public abstract class FatNetLib
{
    protected readonly ILogger? Logger;
    private readonly IEndpointsInvoker _endpointsInvoker;
    protected readonly EventBasedNetListener Listener;
    protected readonly NetManager NetManager;
    protected readonly IEndpointsStorage EndpointsStorage;
    protected readonly List<INetPeer> ConnectedPeers = new();
    private readonly IResponsePackageMonitor _responsePackageMonitor;

    private bool _isStop;

    private const BindingFlags EndpointSearch = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public;

    protected abstract Configuration Configuration { get; }

    protected FatNetLib(ILogger? logger,
        IEndpointsStorage endpointsStorage,
        IEndpointsInvoker endpointsInvoker,
        EventBasedNetListener listener,
        IResponsePackageMonitor responsePackageMonitor)
    {
        Logger = logger;
        EndpointsStorage = endpointsStorage;
        _endpointsInvoker = endpointsInvoker;
        Listener = listener;
        NetManager = new NetManager(Listener);
        _responsePackageMonitor = responsePackageMonitor;
    }

    public delegate void ReceiverDelegate(Package package);

    public delegate Package ExchangerDelegate(Package package);

    public void AddController(IController controller)
    {
        Type controllerType = controller.GetType();
        object[] controllerAttributes = controllerType.GetCustomAttributes(inherit: false);
        string mainPath = "";
        foreach (var attribute in controllerAttributes)
        {
            if (attribute is not Route route) continue;
            string path = route.Path;
            if (string.IsNullOrWhiteSpace(path))
                throw new FatNetLibException($"{controllerType.FullName} path is null or blank");
            mainPath = path;
        }

        foreach (var method in controllerType.GetMethods(EndpointSearch))
        {
            LocalEndpoint localEndpoint = CreateLocalEndpointFromMethod(method, controller, mainPath);
            EndpointsStorage.LocalEndpoints.Add(localEndpoint);
        }
    }

    public void AddReceiver(string route, DeliveryMethod deliveryMethod, ReceiverDelegate receiverDelegate)
    {
        var endpoint = new Endpoint(route, EndpointType.Receiver, deliveryMethod);

        if (EndpointsStorage.LocalEndpoints.Any(_ => _.EndpointData.Path == route))
            throw new FatNetLibException($"Endpoint with the path : {route} was already registered");

        var localEndpoint = new LocalEndpoint(endpoint, receiverDelegate);
        EndpointsStorage.LocalEndpoints.Add(localEndpoint);
    }

    public void AddExchanger(string route, DeliveryMethod deliveryMethod, ExchangerDelegate exchangerDelegate)
    {
        var endpoint = new Endpoint(route, EndpointType.Exchanger, deliveryMethod);

        if (EndpointsStorage.LocalEndpoints.Any(_ => _.EndpointData.Path == route))
            throw new FatNetLibException($"Endpoint with the path : {route} was already registered");

        var localEndpoint = new LocalEndpoint(endpoint, exchangerDelegate);
        EndpointsStorage.LocalEndpoints.Add(localEndpoint);
    }

    public abstract void Run();

    protected void StartListen()
    {
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
                    InvokeEndpoint(package, fromPeer.Id, deliveryMethod);
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

    public Package? SendPackage(Package package, int receivingPeerId)
    {
        if (package == null) throw new ArgumentNullException(nameof(package));

        INetPeer receivingPeer = ConnectedPeers.FirstOrDefault(peer => peer.Id == receivingPeerId) ??
                                 throw new FatNetLibException("Receiving peer not found");

        Endpoint endpoint = EndpointsStorage.RemoteEndpoints[receivingPeerId]
                                .FirstOrDefault(endpoint => endpoint.Path == package.Route) ??
                            throw new FatNetLibException("Endpoint not found");

        if (endpoint.EndpointType == EndpointType.Exchanger && package.ExchangeId == null)
        {
            package = new Package(package) {ExchangeId = Guid.NewGuid()};
        }

        DeliveryMethod deliveryMethod = endpoint.DeliveryMethod;

        SendMessage(package, receivingPeer.Id, deliveryMethod);

        if (endpoint.EndpointType == EndpointType.Receiver)
            return null;

        Guid exchangeId = package.ExchangeId!.Value;
        return _responsePackageMonitor.Wait(exchangeId);
    }

    private void InvokeEndpoint(Package requestPackage, int peerId, DeliveryMethod deliveryMethod)
    {
        LocalEndpoint? endpoint = EndpointsStorage.LocalEndpoints
            .FirstOrDefault(_ => _.EndpointData.Path == requestPackage.Route!);
        if (endpoint == null)
        {
            throw new FatNetLibException(
                $"Package from {peerId} pointed to a non-existent endpoint. Route: {requestPackage.Route}");
        }

        if (endpoint.EndpointData.DeliveryMethod != deliveryMethod)
        {
            throw new FatNetLibException($"Package from {peerId} came with the wrong type of delivery");
        }

        if (endpoint.EndpointData.EndpointType != EndpointType.Exchanger)
        {
            _endpointsInvoker.InvokeReceiver(endpoint, requestPackage);
            return;
        }

        Package responsePackage = _endpointsInvoker.InvokeExchanger(endpoint, requestPackage);

        responsePackage = new Package(responsePackage)
        {
            Route = requestPackage.Route, ExchangeId = requestPackage.ExchangeId, IsResponse = true
        };
        SendMessage(responsePackage, peerId, deliveryMethod);
    }

    //TODO move to SendPackage() when add connection endpoints
    protected void SendMessage(Package package, int peerId, DeliveryMethod deliveryMethod)
    {
        var peer = ConnectedPeers.Single(netPeer => netPeer.Id == peerId);
        string jsonPackage = JsonConvert.SerializeObject(package);
        var writer = new NetDataWriter();
        writer.Put(jsonPackage);
        peer.Send(writer, deliveryMethod);
    }

    private LocalEndpoint CreateLocalEndpointFromMethod(MethodInfo method, IController controller, string? mainPath)
    {
        object[] methodAttributes = method.GetCustomAttributes(inherit: true);
        string? methodPath = null;
        EndpointType? endpointType = null;
        DeliveryMethod? deliveryMethod = null;
        foreach (var attribute in methodAttributes)
        {
            switch (attribute)
            {
                case Route route:
                    string path = route.Path;
                    if (string.IsNullOrWhiteSpace(path))
                        throw new FatNetLibException(
                            $"{method.Name} path in {controller.GetType().Name} is null or blank");
                    methodPath = path;
                    break;

                case Receiver receiver:
                    endpointType = EndpointType.Receiver;
                    deliveryMethod = receiver.DeliveryMethod;
                    break;

                case Exchanger exchanger:
                    if (method.ReturnType != typeof(Package))
                        throw new FatNetLibException(
                            $"Return type of a {method.Name} in a {controller.GetType().Name} must be Package");

                    endpointType = EndpointType.Exchanger;
                    deliveryMethod = exchanger.DeliveryMethod;
                    break;
            }
        }

        if (methodPath == null)
            throw new FatNetLibException(
                $"{method.Name} in {controller.GetType().Name} does not have route attribute");

        if (endpointType == null)
            throw new FatNetLibException(
                $"{method.Name} in {controller.GetType().Name} does not have endpoint type attribute");

        string fullPath = mainPath + "/" + methodPath;

        if (EndpointsStorage.LocalEndpoints.Any(_ => _.EndpointData.Path == fullPath))
            throw new FatNetLibException(
                $"Endpoint with the path : {fullPath} was already registered");

        Delegate methodDelegate = CreateDelegateFromControllerMethod(method, controller);

        return new LocalEndpoint(new Endpoint(fullPath, endpointType.Value, deliveryMethod!.Value), methodDelegate);
    }

    private Delegate CreateDelegateFromControllerMethod(MethodInfo methodInfo, IController controller)
    {
        IEnumerable<Type> paramTypes = methodInfo.GetParameters().Select(parameter => parameter.ParameterType);
        Type delegateType = Expression.GetDelegateType(paramTypes.Append(methodInfo.ReturnType).ToArray());
        Delegate methodDelegate = methodInfo.CreateDelegate(delegateType, controller);

        return methodDelegate;
    }
}