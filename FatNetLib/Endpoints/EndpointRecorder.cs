using System.Linq.Expressions;
using System.Reflection;
using Kolyhalov.FatNetLib.Attributes;
using Kolyhalov.FatNetLib.Microtypes;
using LiteNetLib;

namespace Kolyhalov.FatNetLib.Endpoints;

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
        var mainRoute = Route.Empty;
        foreach (object attribute in controllerAttributes)
        {
            if (attribute is not RouteAttribute route) continue;
            mainRoute += route.Route;
        }

        foreach (MethodInfo method in controllerType.GetMethods(EndpointSearch))
        {
            LocalEndpoint localEndpoint = CreateLocalEndpointFromMethod(method, controller, mainRoute);
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
        var endpoint = new Endpoint(new Route(route), endpointType, deliveryMethod);

        if (_endpointsStorage.LocalEndpoints.Any(_ => _.EndpointData.Route.Equals(endpoint.Route)))
            throw new FatNetLibException($"Endpoint with the route : {endpoint.Route} was already registered");

        var localEndpoint = new LocalEndpoint(endpoint, endpointDelegate);
        _endpointsStorage.LocalEndpoints.Add(localEndpoint);
    }

    private LocalEndpoint CreateLocalEndpointFromMethod(MethodInfo method, IController controller, Route mainRoute)
    {
        object[] methodAttributes = method.GetCustomAttributes(inherit: true);
        var methodRoute = Route.Empty;
        EndpointType? endpointType = null;
        DeliveryMethod? deliveryMethod = null;
        foreach (object attribute in methodAttributes)
        {
            switch (attribute)
            {
                case RouteAttribute routeAttribute:
                    methodRoute += routeAttribute.Route;
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

        if (methodRoute.IsEmpty)
            throw new FatNetLibException(
                $"{method.Name} in {controller.GetType().Name} does not have route attribute");

        if (endpointType == null)
            throw new FatNetLibException(
                $"{method.Name} in {controller.GetType().Name} does not have endpoint type attribute");

        Route fullRoute = mainRoute + methodRoute;

        if (_endpointsStorage.LocalEndpoints.Any(_ => _.EndpointData.Route.Equals(fullRoute)))
            throw new FatNetLibException($"Endpoint with the route {fullRoute} was already registered");

        Delegate methodDelegate = CreateDelegateFromMethod(method, controller);
        var endpoint = new Endpoint(fullRoute, endpointType.Value, deliveryMethod!.Value);
        return new LocalEndpoint(endpoint, methodDelegate);
    }

    private static Delegate CreateDelegateFromMethod(MethodInfo methodInfo, IController controller)
    {
        IEnumerable<Type> paramTypes = methodInfo.GetParameters().Select(parameter => parameter.ParameterType);
        Type delegateType = Expression.GetDelegateType(paramTypes.Append(methodInfo.ReturnType).ToArray());
        return methodInfo.CreateDelegate(delegateType, controller);
    }
}