using System.Linq.Expressions;
using System.Reflection;
using Kolyhalov.FatNetLib.Attributes;
using Kolyhalov.FatNetLib.Endpoints;
using LiteNetLib;

namespace Kolyhalov.FatNetLib;

public class EndpointRecorder : IEndpointRecorder
{
    public IEndpointsStorage EndpointsStorage { get; } = new EndpointsStorage();

    private const BindingFlags EndpointSearch = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public;

    public void AddController(IController controller)
    {
        Type controllerType = controller.GetType();
        object[] controllerAttributes = controllerType.GetCustomAttributes(inherit: false);
        var mainPath = "";
        foreach (object attribute in controllerAttributes)
        {
            if (attribute is not Route route) continue;
            string path = route.Path;
            if (string.IsNullOrWhiteSpace(path))
                throw new FatNetLibException($"{controllerType.FullName} path is null or blank");
            mainPath = path;
        }

        foreach (MethodInfo method in controllerType.GetMethods(EndpointSearch))
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
    
    private static Delegate CreateDelegateFromControllerMethod(MethodInfo methodInfo, IController controller)
    {
        IEnumerable<Type> paramTypes = methodInfo.GetParameters().Select(parameter => parameter.ParameterType);
        Type delegateType = Expression.GetDelegateType(paramTypes.Append(methodInfo.ReturnType).ToArray());
        Delegate methodDelegate = methodInfo.CreateDelegate(delegateType, controller);

        return methodDelegate;
    }
}