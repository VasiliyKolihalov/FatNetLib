using System.Reflection;
using System.Text.Json.Serialization;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace UdpFramework;

public class UdpFramework
{
    private readonly ServerConfiguration? _serverConfiguration;
    private readonly ClientConfiguration? _clientConfiguration;
    private readonly ILogger _logger;

    private readonly EventBasedNetListener _listener = new();
    private readonly NetManager _netManager;

    private readonly Dictionary<NetPeer, List<Endpoint>> _remoteEndpoints = new();
    private readonly List<LocalEndpoint> _localEndpoints = new();

    private bool _isStop;

    public UdpFramework(ServerConfiguration serverConfiguration, ILogger logger)
    {
        _serverConfiguration = serverConfiguration;
        _logger = logger;

        _netManager = new NetManager(_listener);
    }

    public UdpFramework(ClientConfiguration clientConfiguration, ILogger logger)
    {
        _clientConfiguration = clientConfiguration;
        _logger = logger;

        _netManager = new NetManager(_listener);
    }

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

        foreach (var method in type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public))
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
                            throw new UdpFrameworkException($"Return type of a {method.Name} in a {type.Name} must be package");

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
            _localEndpoints.Add(new LocalEndpoint(new Endpoint(fullPath, endpointType.Value, deliveryMethod!.Value), controller, method));
        }
    }

    public void AddReceiver(string route, DeliveryMethod deliveryMethod, ReceiverDelegate receiverDelegate)
    {
        if (string.IsNullOrEmpty(route))
        {
            throw new Exception($"path is null or empty");
        }

        var endpoint = new LocalEndpoint(new Endpoint(route, EndpointType.Receiver, deliveryMethod), controller: null!, receiverDelegate.Method);
        _localEndpoints.Add(endpoint);
    }

    public void AddExchanger(string route, DeliveryMethod deliveryMethod, ExchangerDelegate exchangerDelegate)
    {
        if (string.IsNullOrEmpty(route))
        {
            throw new UdpFrameworkException($"path is null or empty");
        }

        var endpoint = new LocalEndpoint(new Endpoint(route, EndpointType.Exchanger, deliveryMethod), controller: null!, exchangerDelegate.Method);
        _localEndpoints.Add(endpoint);
    }

    public void Run()
    {
        Task.Run(() =>
        {
            if (_serverConfiguration == null)
            {
                _netManager.Start();
                _netManager.Connect(_clientConfiguration!.Address, _clientConfiguration.Port, _clientConfiguration.ConnectionKey);

                _listener.PeerConnectedEvent += peer =>
                {
                    Package endpointsRequest = new Package
                    {
                        Route = "/connection/get-endpoints",
                        Body = new Dictionary<string, object>
                        {
                            ["Endpoits"] = _localEndpoints.Select(x => x.Data)
                        }
                    };
                    SendMessage(endpointsRequest, peer, DeliveryMethod.ReliableSequenced);
                };
            }
            else
            {
                _netManager.Start(_serverConfiguration.Port);

                _listener.ConnectionRequestEvent += request =>
                {
                    if (_netManager.ConnectedPeersCount < _serverConfiguration.MaxPeersCount)
                        request.AcceptIfKey(_serverConfiguration.ConnectionKey);
                    else
                        request.Reject();
                };
            }

            _listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                string jsonPackage = dataReader.GetString();
                Package package = JsonConvert.DeserializeObject<Package>(jsonPackage)!;
                if (package.Route == "/connection/get-endpoints")
                {
                    _remoteEndpoints[fromPeer] = JsonConvert.DeserializeObject<List<Endpoint>>(package.Body["Endpoits"].ToString()!)!;
                    Package responsePackage = new Package
                    {
                        Route = "/connection/hold-endpoints",
                        Body = new Dictionary<string, object>
                        {
                            ["Endpoits"] = _localEndpoints.Select(x => x.Data)
                        }
                    };
                    SendMessage(responsePackage, fromPeer, DeliveryMethod.ReliableSequenced);
                    return;
                }

                if (package.Route == "/connection/hold-endpoints")
                {
                    _remoteEndpoints[fromPeer] = JsonConvert.DeserializeObject<List<Endpoint>>(package.Body["Endpoits"].ToString()!)!;
                    return;
                }

                InvokeEndpoint(package, fromPeer, deliveryMethod);
            };
            
            int framerate = _serverConfiguration?.Framerate ?? _clientConfiguration!.Framerate;
            while (!_isStop)
            {
                _netManager.PollEvents();
                Thread.Sleep(TimeSpan.FromSeconds(1) / (framerate));
            }
        });
    }

    public void Stop()
    {
        _isStop = true;
        _netManager.Stop();
    }

    public Package? SendRequest(Package package, int receiverId)
    {
        NetPeer? receiver = _netManager.ConnectedPeerList.FirstOrDefault(x => x.Id == receiverId);
        if (receiver == null)
            throw new UdpFrameworkException("Receiver not found");

        Endpoint? endpointData = _remoteEndpoints[receiver].FirstOrDefault(x => x.Path == package.Route);
        if (endpointData == null)
            throw new UdpFrameworkException("Endpoint not found");

        DeliveryMethod deliveryMethod = endpointData.DeliveryMethod;

        SendMessage(package, receiver, deliveryMethod);

        if (endpointData.EndpointType == EndpointType.Receiver)
            return null;

        Package? responsePackage = null;
        // дождаться ответа.
        return responsePackage;
    }

    private void InvokeEndpoint(Package package, NetPeer peer, DeliveryMethod deliveryMethod)
    {
        if (string.IsNullOrEmpty(package.Route))
        {
            _logger.LogError($"Package came from {peer.Id} without route");
            return;
        }

        LocalEndpoint? endpoint = _localEndpoints.FirstOrDefault(x => x.Data.Path == package.Route);
        if (endpoint == null)
        {
            _logger.LogError($"Request from {peer.Id} pointed to a non-existent endpoint. Route: {package.Route}");
            return;
        }

        if (endpoint.Data.DeliveryMethod != deliveryMethod)
        {
            _logger.LogError($"Request from {peer.Id} came with the wrong type of delivery");
            return;
        }

        List<Type> parameterTypes = new List<Type>();
        foreach (var parameterType in endpoint.Method.GetParameters())
        {
            parameterTypes.Add(parameterType.ParameterType);
        }

        List<object> parameters = new List<object>();

        foreach (var parameterType in parameterTypes)
        {
            try
            {
                if (endpoint.Controller == null)
                {
                    parameters.Add(package);
                    break;
                }

                parameters.Add(JsonConvert.DeserializeObject(package.Body[parameterType.Name].ToString()!,
                    parameterType)!);
            }
            catch
            {
                _logger.LogError($"There is no required field: {parameterType.Name} in the package from {peer.Id}");
                return;
            }
        }

        if (endpoint.Data.EndpointType == EndpointType.Receiver)
        {
            if (endpoint.Controller == null)
            {
                ReceiverDelegate receiverDelegate = (ReceiverDelegate) Delegate.CreateDelegate(typeof(ReceiverDelegate), null, endpoint.Method);
                receiverDelegate.Invoke(package);
                return;
            }

            endpoint.Method.Invoke(endpoint.Controller, parameters.ToArray());
            return;
        }

        if (endpoint.Data.EndpointType == EndpointType.Exchanger)
        {
            Package responsePackage;
            if (endpoint.Controller == null)
            {
                ExchangerDelegate exchangerDelegate = (ExchangerDelegate) Delegate.CreateDelegate(typeof(ExchangerDelegate), null, endpoint.Method);
                responsePackage = exchangerDelegate.Invoke(package);
            }
            else
            {
                responsePackage = (endpoint.Method.Invoke(endpoint.Controller, parameters.ToArray()) as Package)!;
            }

            SendMessage(responsePackage, peer, deliveryMethod);
        }
    }

    private static void SendMessage(Package package, NetPeer peer, DeliveryMethod deliveryMethod)
    {
        string jsonPackage = JsonConvert.SerializeObject(package);
        NetDataWriter writer = new NetDataWriter();
        writer.Put(jsonPackage);
        peer.Send(writer, deliveryMethod);
    }
}