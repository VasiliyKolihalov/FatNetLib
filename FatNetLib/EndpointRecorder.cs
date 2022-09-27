using System.Linq.Expressions;
using System.Reflection;
using Kolyhalov.FatNetLib.Attributes;
using Kolyhalov.FatNetLib.Endpoints;
using LiteNetLib;

namespace Kolyhalov.FatNetLib;

public class EndpointRecorder : IEndpointRecorder
{
    private readonly IEndpointsStorage _endpointsStorage;

    public EndpointRecorder(IEndpointsStorage endpointsStorage)
    {
        _endpointsStorage = endpointsStorage;
    }

    private const BindingFlags EndpointSearch = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public;

    public void AddController(IController controller)
    {
        Type controllerType = controller.GetType();
        object[] controllerAttributes = controllerType.GetCustomAttributes(inherit: false);
        Path? mainPath = null;
        foreach (object attribute in controllerAttributes)
        {
            if (attribute is not Route route) continue;
            mainPath = new Path(route.Path);
        }

        foreach (MethodInfo method in controllerType.GetMethods(EndpointSearch))
        {
            LocalEndpoint localEndpoint = CreateLocalEndpointFromMethod(method, controller, mainPath);
            _endpointsStorage.LocalEndpoints.Add(localEndpoint);
        }
    }

    public void AddReceiver(string route, DeliveryMethod deliveryMethod, ReceiverDelegate receiverDelegate)
    {
        AddEndpoint(route, deliveryMethod, receiverDelegate, EndpointType.Receiver);
    }

    public void AddExchanger(string route, DeliveryMethod deliveryMethod, ExchangerDelegate exchangerDelegate)
    {
        AddEndpoint(route, deliveryMethod, exchangerDelegate, EndpointType.Exchanger);
    }

    private void AddEndpoint(string route,
        DeliveryMethod deliveryMethod,
        Delegate endpointDelegate,
        EndpointType endpointType)
    {
        var endpoint = new Endpoint(new Path(route), endpointType, deliveryMethod);

        if (_endpointsStorage.LocalEndpoints.Any(_ => _.EndpointData.Path.Equals(endpoint.Path)))
            throw new FatNetLibException($"Endpoint with the path was already registered");

        var localEndpoint = new LocalEndpoint(endpoint, endpointDelegate);
        _endpointsStorage.LocalEndpoints.Add(localEndpoint);
    }

    private LocalEndpoint CreateLocalEndpointFromMethod(MethodInfo method, IController controller, Path? mainPath)
    {
        object[] methodAttributes = method.GetCustomAttributes(inherit: true);
        Path methodPath = null!;
        EndpointType? endpointType = null;
        DeliveryMethod? deliveryMethod = null;
        foreach (object attribute in methodAttributes)
        {
            switch (attribute)
            {
                case Route route:
                    methodPath = new Path(route.Path);
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

        var path = new List<string>();
        if (mainPath != null)
            path.AddRange(mainPath.Value);
        path.AddRange(methodPath.Value);

        var fullPath = new Path(path);

        if (_endpointsStorage.LocalEndpoints.Any(_ => _.EndpointData.Path.Equals(fullPath)))
            throw new FatNetLibException("Endpoint with the path was already registered");

        Delegate methodDelegate = CreateDelegateFromMethod(method, controller);
        Endpoint endpoint = new Endpoint(fullPath, endpointType.Value, deliveryMethod!.Value);
        return new LocalEndpoint(endpoint, methodDelegate);
    }

    private static Delegate CreateDelegateFromMethod(MethodInfo methodInfo, IController controller)
    {
        IEnumerable<Type> paramTypes = methodInfo.GetParameters().Select(parameter => parameter.ParameterType);
        Type delegateType = Expression.GetDelegateType(paramTypes.Append(methodInfo.ReturnType).ToArray());
        return methodInfo.CreateDelegate(delegateType, controller);
    }
}