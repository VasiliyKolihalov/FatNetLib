using System.Text.Json;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Extensions.Logging;

namespace UdpFramework;

public class UdpFramework
{
    private readonly int _port;
    private readonly string _connectionKey;
    private readonly int _framerate;
    private readonly int _maxPeersCount;
    private readonly ILogger _logger;

    private readonly EventBasedNetListener _listener;
    private readonly NetManager _server;

    private readonly List<NetPeer> _clients;
    private readonly Dictionary<NetPeer, List<EndpointData>> _clientsEndpoints;
    private readonly List<RegisterEndpoint> _serverEndpoints;

    public UdpFramework(int port, string connectionKey, int framerate, int maxPeersCount, ILogger logger)
    {
        _port = port;
        _connectionKey = connectionKey;
        _framerate = framerate;
        _maxPeersCount = maxPeersCount;
        _logger = logger;

        _listener = new EventBasedNetListener();
        _server = new NetManager(_listener);

        _clients = new List<NetPeer>();
        _clientsEndpoints = new Dictionary<NetPeer, List<EndpointData>>();
        _serverEndpoints = new List<RegisterEndpoint>();
    }

    public delegate void ReceiverDelegate(Package package);

    public delegate Package ExchangerDelegate(Package package);

    public void AddController(Controller controller)
    {
        Type type = typeof(Controller);
        object[] controllerAttributes = type.GetCustomAttributes(true);
        string? mainPath = null;
        foreach (var attribute in controllerAttributes)
        {
            if (attribute is not Route route) continue;
            string path = route.Path;
            if (string.IsNullOrEmpty(path))
                throw new Exception($"{type.FullName} path is null or empty");
            mainPath = path;
        }

        if (mainPath == null)
            throw new Exception($"{type.FullName} does not have route attribute");

        foreach (var method in type.GetMethods())
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
                            throw new Exception($"{method.Name} path in {type.Name} is null or empty");
                        methodPath = path;
                        break;

                    case Receiver receiver:
                        endpointType = EndpointType.Receiver;
                        deliveryMethod = receiver.DeliveryMethod;
                        break;

                    case Exchanger exchanger:
                        if (method.ReturnType != typeof(Package))
                            throw new Exception($"Return type of a {method.Name} in a {type.Name} must be package");

                        endpointType = EndpointType.Exchanger;
                        deliveryMethod = exchanger.DeliveryMethod;
                        break;
                }
            }

            if (methodPath == null)
                throw new Exception($"{method.Name} in {type.Name} does not have route attribute");

            if (endpointType == null)
                throw new Exception($"{method.Name} in {type.Name} does not have endpoint type attribute");

            string fullPath = mainPath + "/" + methodPath;
            _serverEndpoints.Add(new RegisterEndpoint(fullPath, endpointType.Value, deliveryMethod!.Value, controller, method));
        }
    }

    public void AddReceiver(string route, DeliveryMethod deliveryMethod, ReceiverDelegate receiverDelegate)
    {
        if (string.IsNullOrEmpty(route))
        {
            throw new Exception($"path is null or empty");
        }

        RegisterEndpoint endpoint = new RegisterEndpoint(route, EndpointType.Receiver, deliveryMethod, null!,
            receiverDelegate.Method);
        _serverEndpoints.Add(endpoint);
    }

    public void AddExchanger(string route, DeliveryMethod deliveryMethod, ExchangerDelegate exchangerDelegate)
    {
        if (string.IsNullOrEmpty(route))
        {
            throw new Exception($"path is null or empty");
        }

        RegisterEndpoint endpoint = new RegisterEndpoint(route, EndpointType.Receiver, deliveryMethod, null!,
            exchangerDelegate.Method);
        _serverEndpoints.Add(endpoint);
    }

    public void Run()
    {
        _server.Start(_port);
        _listener.ConnectionRequestEvent += request =>
        {
            if (_server.ConnectedPeersCount < _maxPeersCount)
                request.AcceptIfKey(_connectionKey);
            else
                request.Reject();
        };

        _listener.PeerConnectedEvent += peer =>
        {
            //получить эндпоинты клиента
            _clients.Add(peer);
        };

        _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
        {
            string jsonPackage = dataReader.GetString();
            Package package = JsonSerializer.Deserialize<Package>(jsonPackage)!;
            InvokeEndpoint(package, fromPeer, deliveryMethod);
        };
        while (true)
        {
            Thread.Sleep(TimeSpan.FromSeconds(1) / _framerate);
            _server.PollEvents();
        }
    }

    public Package? SendRequest(Package package, int receiverId)
    {
        NetPeer? receiver = _clients.FirstOrDefault(x => x.Id == receiverId);
        if (receiver == null)
            throw new Exception("Receiver not found");

        EndpointData? endpointData = _clientsEndpoints[receiver].FirstOrDefault(x => x.Path == package.Route);
        if (endpointData == null)
            throw new Exception("Endpoint not found");

        DeliveryMethod deliveryMethod = endpointData.DeliveryMethod;
        string packageJson = JsonSerializer.Serialize(receiver);

        NetDataWriter netDataWriter = new NetDataWriter();
        netDataWriter.Put(packageJson);

        receiver.Send(netDataWriter, deliveryMethod);

        if (endpointData.EndpointType == EndpointType.Receiver)
            return null;

        Package? responsePackage = null;
        if (endpointData.EndpointType == EndpointType.Exchanger)
        {
            void Handler(NetPeer peer, NetPacketReader reader, DeliveryMethod method)
            {
                string jsonPackage = reader.GetString();
                //Package ... = JsonSerializer.Deserialize<Package>(jsonPackage)!;
                if (false) // является ли пакет ответом? 
                {
                    //responsePackage = ...;
                    _listener.NetworkReceiveEvent -= Handler;
                }
            }

            _listener.NetworkReceiveEvent += Handler;
        }

        while (true)
        {
            Thread.Sleep(TimeSpan.FromSeconds(1) / _framerate);
            _server.PollEvents();
            if (responsePackage != null)
                return responsePackage;
        }
    }

    private void InvokeEndpoint(Package package, NetPeer client, DeliveryMethod deliveryMethod)
    {
        if (string.IsNullOrEmpty(package.Route))
        {
            _logger.LogError($"Package came from {client.Id} without root");
            return;
        }

        RegisterEndpoint? endpoint = _serverEndpoints.FirstOrDefault(x => x.Data.Path == package.Route);
        if (endpoint == null)
        {
            _logger.LogError($"Request from {client.Id} pointed to a non-existent endpoint");
            return;
        }

        if (endpoint.Data.DeliveryMethod != deliveryMethod)
        {
            _logger.LogError($"Request from {client.Id} came with the wrong type of delivery");
            return;
        }

        List<Type> parameterTypes = new List<Type>();
        foreach (var parameter in endpoint.Method.GetParameters())
        {
            parameterTypes.Add(parameter.ParameterType);
        }

        List<object> parameters = new List<object>();

        foreach (var parameter in parameterTypes)
        {
            try
            {
                parameters.Add(package.Body[parameter.Name]);
            }
            catch
            {
                _logger.LogError($"There is no required field: {parameter.Name} in the package from {client.Id}");
                return;
            }
        }

        if (endpoint.Data.EndpointType == EndpointType.Receiver)
        {
            endpoint.Method.Invoke(endpoint.Controller, parameters.ToArray());
            return;
        }

        if (endpoint.Data.EndpointType == EndpointType.Exchanger)
        {
            Package responsePackage =
                (endpoint.Method.Invoke(endpoint.Controller, parameters.ToArray()) as Package)!;
            // отправить ответ
        }
    }
}