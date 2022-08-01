﻿using System.Linq.Expressions;
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
    private readonly IEndpointsInvoker _endpointsInvoker;

    protected readonly EventBasedNetListener Listener;
    protected readonly NetManager NetManager;

    protected readonly IEndpointsStorage EndpointsStorage;

    protected readonly List<INetPeerShell> ConnectedPeers = new();

    private bool _isStop;

    private const BindingFlags EndpointSearch = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public;

    protected UdpFramework(ILogger logger, IEndpointsStorage endpointsStorage, IEndpointsInvoker endpointsInvoker,
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
        object[] controllerAttributes = controllerType.GetCustomAttributes(false);
        string mainPath = "";
        foreach (var attribute in controllerAttributes)
        {
            if (attribute is not Route route) continue;
            string path = route.Path;
            if (string.IsNullOrEmpty(path))
                throw new UdpFrameworkException($"{controllerType.FullName} path is null or empty");
            mainPath = path;
        }

        foreach (var method in controllerType.GetMethods(EndpointSearch))
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
                            throw new UdpFrameworkException(
                                $"{method.Name} path in {controllerType.Name} is null or empty");
                        methodPath = path;
                        break;

                    case Receiver receiver:
                        endpointType = EndpointType.Receiver;
                        deliveryMethod = receiver.DeliveryMethod;
                        break;

                    case Exchanger exchanger:
                        if (method.ReturnType != typeof(Package))
                            throw new UdpFrameworkException(
                                $"Return type of a {method.Name} in a {controllerType.Name} must be package");

                        endpointType = EndpointType.Exchanger;
                        deliveryMethod = exchanger.DeliveryMethod;
                        break;
                }
            }

            if (methodPath == null)
                throw new UdpFrameworkException(
                    $"{method.Name} in {controllerType.Name} does not have route attribute");

            if (endpointType == null)
                throw new UdpFrameworkException(
                    $"{method.Name} in {controllerType.Name} does not have endpoint type attribute");

            string fullPath = mainPath + "/" + methodPath;

            if (EndpointsStorage.GetLocalEndpointFromPath(fullPath) != null)
                throw new UdpFrameworkException($"Endpoint with the path : {fullPath} was already registered");

            Delegate methodDelegate = CreateDelegateFromControllerMethod(method, controller);

            EndpointsStorage.AddLocalEndpoint(new LocalEndpoint(
                new Endpoint(fullPath, endpointType.Value, deliveryMethod!.Value),
                methodDelegate));
        }
    }

    public void AddReceiver(string route, DeliveryMethod deliveryMethod, ReceiverDelegate receiverDelegate)
    {
        if (route == null) throw new ArgumentNullException(nameof(route));
        if (receiverDelegate == null) throw new ArgumentNullException(nameof(receiverDelegate));

        var endpoint = new Endpoint(route, EndpointType.Receiver, deliveryMethod);
        endpoint.Validate();

        if (EndpointsStorage.GetLocalEndpointFromPath(route) != null)
            throw new UdpFrameworkException($"Endpoint with the path : {route} was already registered");

        var localEndpoint = new LocalEndpoint(endpoint, receiverDelegate);

        EndpointsStorage.AddLocalEndpoint(localEndpoint);
    }

    public void AddExchanger(string route, DeliveryMethod deliveryMethod, ExchangerDelegate exchangerDelegate)
    {
        if (route == null) throw new ArgumentNullException(nameof(route));
        if (exchangerDelegate == null) throw new ArgumentNullException(nameof(exchangerDelegate));

        var endpoint = new Endpoint(route, EndpointType.Exchanger, deliveryMethod);
        endpoint.Validate();

        if (EndpointsStorage.GetLocalEndpointFromPath(route) != null)
            throw new UdpFrameworkException($"Endpoint with the path : {route} was already registered");

        var localEndpoint = new LocalEndpoint(endpoint, exchangerDelegate);

        EndpointsStorage.AddLocalEndpoint(localEndpoint);
    }

    public abstract void Run();

    protected void StartListen(int framerate)
    {
        if (_isStop)
            throw new UdpFrameworkException("UdpFramework finished work");

        Listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
        {
            string jsonPackage = dataReader.GetString();
            Package package;
            try
            {
                package = JsonConvert.DeserializeObject<Package>(jsonPackage) ?? 
                          throw new UdpFrameworkException("Failed to deserialize package");
            }
            catch (Exception exception)
            {
                Logger.LogDebug(exception.Message);
                return;
            }
            package.Validate();
            
            if (package.Route!.Contains("connection"))
                return;
            InvokeEndpoint(package, fromPeer.Id, deliveryMethod);
        };

        Listener.PeerConnectedEvent += peer => ConnectedPeers.Add(new NetPeer(peer));

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
        package.Validate();

        INetPeerShell receiver = ConnectedPeers.FirstOrDefault(peer => peer.Id == receiverId) ??
                                 throw new UdpFrameworkException("Receiver not found");

        Endpoint endpoint = EndpointsStorage.GetRemoteEndpoints(receiverId)
            .FirstOrDefault(endpoint => endpoint.Path == package.Route) ?? throw new UdpFrameworkException("Endpoint not found");

        DeliveryMethod deliveryMethod = endpoint.DeliveryMethod;

        SendMessage(package, receiver.Id, deliveryMethod);

        if (endpoint.EndpointType == EndpointType.Receiver)
            return null;

        throw new NotImplementedException("Exchangers not implemented yet");
    }

    private void InvokeEndpoint(Package package, int peerId, DeliveryMethod deliveryMethod)
    {
        LocalEndpoint? endpoint = EndpointsStorage.GetLocalEndpointFromPath(package.Route!);

        if (endpoint == null)
        {
            Logger.LogDebug($"Package from {peerId} pointed to a non-existent endpoint. Route: {package.Route}");
            return;
        }

        if (endpoint.EndpointData.DeliveryMethod != deliveryMethod)
        {
            Logger.LogDebug($"Package from {peerId} came with the wrong type of delivery");
            return;
        }

        Package? responsePackage;

        try
        {
            responsePackage = _endpointsInvoker.InvokeEndpoint(endpoint, package);
        }
        catch (Exception exception)
        {
            Logger.LogDebug(exception.Message);
            return;
        }

        if (responsePackage != null)
        {
            responsePackage.Validate();
            SendMessage(responsePackage, peerId, deliveryMethod);
        }
    }

    protected void SendMessage(Package package, int peerId, DeliveryMethod deliveryMethod)
    {
        var peer = ConnectedPeers.Single(netPeer => netPeer.Id == peerId);
        string jsonPackage = JsonConvert.SerializeObject(package);
        NetDataWriter writer = new NetDataWriter();
        writer.Put(jsonPackage);
        peer.Send(writer, deliveryMethod);
    }

    private Delegate CreateDelegateFromControllerMethod(MethodInfo methodInfo, IController controller)
    {
        IEnumerable<Type> paramTypes = methodInfo.GetParameters().Select(parameter => parameter.ParameterType);

        Type delegateType = Expression.GetDelegateType(paramTypes.Append(methodInfo.ReturnType).ToArray());

        Delegate methodDelegate = methodInfo.CreateDelegate(delegateType, controller);

        return methodDelegate;
    }
}