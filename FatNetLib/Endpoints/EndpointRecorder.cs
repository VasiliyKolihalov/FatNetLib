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
        var isInitial = false;
        foreach (object attribute in controllerAttributes)
        {
            switch (attribute)
            {
                case RouteAttribute route:
                    mainRoute += route.Route;
                    break;
                case InitialAttribute:
                    isInitial = true;
                    break;
            }
        }

        foreach (MethodInfo method in controllerType.GetMethods(EndpointSearch))
        {
            LocalEndpoint localEndpoint = CreateLocalEndpointFromMethod(method, controller, mainRoute, isInitial);
            _endpointsStorage.LocalEndpoints.Add(localEndpoint);
        }
    }

    public void AddReceiver(Route route,
        DeliveryMethod deliveryMethod,
        ReceiverDelegate receiverDelegate,
        PackageSchema? requestSchemaPatch = default)
    {
        AddEndpoint(route,
            deliveryMethod,
            receiverDelegate,
            EndpointType.Receiver,
            requestSchemaPatch: requestSchemaPatch);
    }

    public void AddReceiver(string route,
        DeliveryMethod deliveryMethod,
        ReceiverDelegate receiverDelegate,
        PackageSchema? requestSchemaPatch = default)
    {
        AddEndpoint(new Route(route),
            deliveryMethod,
            receiverDelegate,
            EndpointType.Receiver,
            requestSchemaPatch: requestSchemaPatch);
    }

    public void AddExchanger(Route route,
        DeliveryMethod deliveryMethod,
        ExchangerDelegate exchangerDelegate,
        bool isInitial = false,
        PackageSchema? requestSchemaPatch = default,
        PackageSchema? responseSchemaPatch = default)
    {
        AddEndpoint(route,
            deliveryMethod,
            exchangerDelegate,
            EndpointType.Exchanger,
            isInitial,
            requestSchemaPatch,
            responseSchemaPatch);
    }

    public void AddExchanger(string route,
        DeliveryMethod deliveryMethod,
        ExchangerDelegate exchangerDelegate,
        bool isInitial = false,
        PackageSchema? requestSchemaPatch = default,
        PackageSchema? responseSchemaPatch = default)
    {
        AddEndpoint(new Route(route),
            deliveryMethod,
            exchangerDelegate,
            EndpointType.Exchanger,
            isInitial,
            requestSchemaPatch,
            responseSchemaPatch);
    }

    private void AddEndpoint(Route route,
        DeliveryMethod deliveryMethod,
        Delegate endpointDelegate,
        EndpointType endpointType,
        bool isInitial = false,
        PackageSchema? requestSchemaPatch = default,
        PackageSchema? responseSchemaPatch = default)
    {
        if (route == null)
            throw new ArgumentNullException(nameof(route));

        var endpoint = new Endpoint(route,
            endpointType,
            deliveryMethod,
            isInitial,
            requestSchemaPatch ?? new PackageSchema(),
            responseSchemaPatch ?? new PackageSchema());

        if (_endpointsStorage.LocalEndpoints.Any(_ => _.EndpointData.Route.Equals(endpoint.Route)))
            throw new FatNetLibException($"Endpoint with the route : {endpoint.Route} was already registered");

        var localEndpoint = new LocalEndpoint(endpoint, endpointDelegate);
        _endpointsStorage.LocalEndpoints.Add(localEndpoint);
    }

    private LocalEndpoint CreateLocalEndpointFromMethod(MethodInfo method,
        IController controller,
        Route mainRoute,
        bool isInitial)
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

                case ReceiverAttribute receiver:
                    endpointType = EndpointType.Receiver;
                    deliveryMethod = receiver.DeliveryMethod;
                    break;

                case ExchangerAttribute exchanger:
                {
                    if (method.ReturnType != typeof(Package))
                        throw new FatNetLibException(
                            $"Return type of a {method.Name} in a {controller.GetType().Name} must be Package");

                    if (isInitial && exchanger.DeliveryMethod != DeliveryMethod.ReliableOrdered)
                        throw new FatNetLibException("Initiating endpoint must have reliable ordered type of delivery");

                    endpointType = EndpointType.Exchanger;
                    deliveryMethod = exchanger.DeliveryMethod;
                    break;
                }
            }
        }

        if (methodRoute.IsEmpty)
            throw new FatNetLibException(
                $"{method.Name} in {controller.GetType().Name} does not have route attribute");

        if (endpointType == null)
            throw new FatNetLibException(
                $"{method.Name} in {controller.GetType().Name} does not have endpoint type attribute");

        if (isInitial && endpointType != EndpointType.Exchanger)
            throw new FatNetLibException("All endpoints of initial controller should be exchanger");

        Route fullRoute = mainRoute + methodRoute;

        if (_endpointsStorage.LocalEndpoints.Any(_ => _.EndpointData.Route.Equals(fullRoute)))
            throw new FatNetLibException($"Endpoint with the route {fullRoute} was already registered");

        PackageSchema requestSchemaPatch = CreateRequestSchemaPatch(method);
        PackageSchema responseSchemaPatch = CreateResponseSchemaPatch(method);
        var endpoint = new Endpoint(fullRoute,
            endpointType.Value,
            deliveryMethod!.Value,
            isInitial,
            requestSchemaPatch,
            responseSchemaPatch);
        Delegate methodDelegate = CreateDelegateFromMethod(method, controller);
        return new LocalEndpoint(endpoint, methodDelegate);
    }

    private static PackageSchema CreateRequestSchemaPatch(MethodInfo method)
    {
        var patch = new PackageSchema();
        IEnumerable<SchemaAttribute> schemaAttributes = method.GetCustomAttributes<SchemaAttribute>();
        foreach (SchemaAttribute schemaAttribute in schemaAttributes)
        {
            if (patch.ContainsKey(schemaAttribute.Key))
                throw new FatNetLibException($"Type of {nameof(Package.Body)} is already specified");

            patch[schemaAttribute.Key] = schemaAttribute.Type;
        }

        return patch;
    }

    private static PackageSchema CreateResponseSchemaPatch(MethodInfo method)
    {
        var patch = new PackageSchema();
        IEnumerable<SchemaAttribute> schemaAttributes = method.ReturnParameter.GetCustomAttributes<SchemaAttribute>();
        foreach (SchemaAttribute schemaAttribute in schemaAttributes)
        {
            if (patch.ContainsKey(schemaAttribute.Key))
                throw new FatNetLibException($"Type of {nameof(Package.Body)} is already specified");

            patch[schemaAttribute.Key] = schemaAttribute.Type;
        }

        return patch;
    }

    private static Delegate CreateDelegateFromMethod(MethodInfo methodInfo, IController controller)
    {
        IEnumerable<Type> paramTypes = methodInfo.GetParameters().Select(parameter => parameter.ParameterType);
        Type delegateType = Expression.GetDelegateType(paramTypes.Append(methodInfo.ReturnType).ToArray());
        return methodInfo.CreateDelegate(delegateType, controller);
    }
}