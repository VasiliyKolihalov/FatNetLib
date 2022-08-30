using System.Linq.Expressions;
using System.Reflection;
using Kolyhalov.UdpFramework.Attributes;
using Kolyhalov.UdpFramework.Configurations;
using Kolyhalov.UdpFramework.Endpoints;
using Kolyhalov.UdpFramework.NetPeers;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static Kolyhalov.UdpFramework.ExceptionUtils;

namespace Kolyhalov.UdpFramework;

// todo: add debug logs
// todo: refactor this huge class
public abstract class UdpFramework
{
    // todo: investigate should we inject ILogger or ILoggerFactory, because logger should have name same as class
    protected readonly ILogger? Logger;
    private readonly IEndpointsInvoker _endpointsInvoker;
    protected readonly EventBasedNetListener Listener;
    protected readonly NetManager NetManager;
    protected readonly IEndpointsStorage EndpointsStorage;
    protected readonly List<INetPeer> ConnectedPeers = new();

    private bool _isStop;

    private const BindingFlags EndpointSearch = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public;

    protected abstract Configuration Configuration { get; }

    protected UdpFramework(ILogger? logger, IEndpointsStorage endpointsStorage, IEndpointsInvoker endpointsInvoker,
        EventBasedNetListener listener)
    {
        Logger = logger;
        EndpointsStorage = endpointsStorage;
        _endpointsInvoker = endpointsInvoker;
        Listener = listener;
        NetManager = new NetManager(Listener);
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
                throw new UdpFrameworkException($"{controllerType.FullName} path is null or blank");
            mainPath = path;
        }

        foreach (var method in controllerType.GetMethods(EndpointSearch))
        {
            LocalEndpoint localEndpoint = CreateLocalEndpointFromMethod(method, controller, mainPath);
            EndpointsStorage.AddLocalEndpoint(localEndpoint);
        }
    }

    public void AddReceiver(string route, DeliveryMethod deliveryMethod, ReceiverDelegate receiverDelegate)
    {
        var endpoint = new Endpoint(route, EndpointType.Receiver, deliveryMethod);

        if (EndpointsStorage.GetLocalEndpointByPath(route) != null)
            throw new UdpFrameworkException($"Endpoint with the path : {route} was already registered");

        var localEndpoint = new LocalEndpoint(endpoint, receiverDelegate);
        EndpointsStorage.AddLocalEndpoint(localEndpoint);
    }

    public void AddExchanger(string route, DeliveryMethod deliveryMethod, ExchangerDelegate exchangerDelegate)
    {
        var endpoint = new Endpoint(route, EndpointType.Exchanger, deliveryMethod);

        if (EndpointsStorage.GetLocalEndpointByPath(route) != null)
            throw new UdpFrameworkException($"Endpoint with the path : {route} was already registered");

        var localEndpoint = new LocalEndpoint(endpoint, exchangerDelegate);
        EndpointsStorage.AddLocalEndpoint(localEndpoint);
    }

    public abstract void Run();

    protected void StartListen()
    {
        if (_isStop)
            throw new UdpFrameworkException("UdpFramework finished work");

        Listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            CatchExceptionsTo(Logger, () =>
            {
                string jsonPackage = dataReader.GetString();
                Package package;
                try
                {
                    package = JsonConvert.DeserializeObject<Package>(jsonPackage)
                              ?? throw new UdpFrameworkException("Deserialized package is null");
                }
                catch (Exception exception)
                {
                    throw new UdpFrameworkException("Failed to deserialize package", exception);
                }

                if (package.Route!.Contains("connection"))
                    return;

                InvokeEndpoint(package, fromPeer.Id, deliveryMethod);
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
                                 throw new UdpFrameworkException("Receiving peer not found");

        Endpoint endpoint = EndpointsStorage.GetRemoteEndpoints(receivingPeerId)
                                .FirstOrDefault(endpoint => endpoint.Path == package.Route) ??
                            throw new UdpFrameworkException("Endpoint not found");

        DeliveryMethod deliveryMethod = endpoint.DeliveryMethod;

        SendMessage(package, receivingPeer.Id, deliveryMethod);

        if (endpoint.EndpointType == EndpointType.Receiver)
            return null;

        throw new NotImplementedException("Exchangers not implemented yet");
    }

    private void InvokeEndpoint(Package requestPackage, int peerId, DeliveryMethod deliveryMethod)
    {
        LocalEndpoint? endpoint = EndpointsStorage.GetLocalEndpointByPath(requestPackage.Route!);

        if (endpoint == null)
        {
            throw new UdpFrameworkException(
                $"Package from {peerId} pointed to a non-existent endpoint. Route: {requestPackage.Route}");
        }

        if (endpoint.EndpointData.DeliveryMethod != deliveryMethod)
        {
            throw new UdpFrameworkException($"Package from {peerId} came with the wrong type of delivery");
        }

        // todo: make impossible to create exchanger without a response and a receiver with a response 
        Package? responsePackage = _endpointsInvoker.InvokeEndpoint(endpoint, requestPackage);

        if (responsePackage != null)
        {
            throw new NotImplementedException("Exchangers not implemented yet");
        }
    }

    //TODO move to SendPackage()
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
                        throw new UdpFrameworkException(
                            $"{method.Name} path in {controller.GetType().Name} is null or blank");
                    methodPath = path;
                    break;

                case Receiver receiver:
                    endpointType = EndpointType.Receiver;
                    deliveryMethod = receiver.DeliveryMethod;
                    break;

                case Exchanger exchanger:
                    if (method.ReturnType != typeof(Package))
                        throw new UdpFrameworkException(
                            $"Return type of a {method.Name} in a {controller.GetType().Name} must be Package");

                    endpointType = EndpointType.Exchanger;
                    deliveryMethod = exchanger.DeliveryMethod;
                    break;
            }
        }

        if (methodPath == null)
            throw new UdpFrameworkException(
                $"{method.Name} in {controller.GetType().Name} does not have route attribute");

        if (endpointType == null)
            throw new UdpFrameworkException(
                $"{method.Name} in {controller.GetType().Name} does not have endpoint type attribute");

        string fullPath = mainPath + "/" + methodPath;

        if (EndpointsStorage.GetLocalEndpointByPath(fullPath) != null)
            throw new UdpFrameworkException(
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