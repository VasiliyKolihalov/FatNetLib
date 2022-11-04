using System.Linq.Expressions;
using System.Reflection;
using Kolyhalov.FatNetLib.Attributes;
using Kolyhalov.FatNetLib.Microtypes;
using LiteNetLib;

namespace Kolyhalov.FatNetLib.Endpoints;

public class EndpointRecorder : IEndpointRecorder
{
    private const BindingFlags EndpointSearch = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public;
    private const DeliveryMethod InitialDeliveryMethod = DeliveryMethod.ReliableOrdered;

    private readonly IEndpointsStorage _endpointsStorage;

    public EndpointRecorder(IEndpointsStorage endpointsStorage)
    {
        _endpointsStorage = endpointsStorage;
    }

    public IEndpointRecorder AddReceiver(
        Route route,
        DeliveryMethod deliveryMethod,
        ReceiverDelegate receiverDelegate,
        PackageSchema? requestSchemaPatch = default)
    {
        AddEndpoint(
            route,
            deliveryMethod,
            receiverDelegate,
            EndpointType.Receiver,
            requestSchemaPatch: requestSchemaPatch);
        return this;
    }

    public IEndpointRecorder AddReceiver(
        string route,
        DeliveryMethod deliveryMethod,
        ReceiverDelegate receiverDelegate,
        PackageSchema? requestSchemaPatch = default)
    {
        AddEndpoint(
            new Route(route),
            deliveryMethod,
            receiverDelegate,
            EndpointType.Receiver,
            requestSchemaPatch: requestSchemaPatch);
        return this;
    }

    public IEndpointRecorder AddExchanger(
        Route route,
        DeliveryMethod deliveryMethod,
        ExchangerDelegate exchangerDelegate,
        PackageSchema? requestSchemaPatch = default,
        PackageSchema? responseSchemaPatch = default)
    {
        AddEndpoint(
            route,
            deliveryMethod,
            exchangerDelegate,
            EndpointType.Exchanger,
            isInitial: false,
            requestSchemaPatch,
            responseSchemaPatch);
        return this;
    }

    public IEndpointRecorder AddExchanger(
        string route,
        DeliveryMethod deliveryMethod,
        ExchangerDelegate exchangerDelegate,
        PackageSchema? requestSchemaPatch = default,
        PackageSchema? responseSchemaPatch = default)
    {
        AddEndpoint(
            new Route(route),
            deliveryMethod,
            exchangerDelegate,
            EndpointType.Exchanger,
            isInitial: false,
            requestSchemaPatch,
            responseSchemaPatch);
        return this;
    }

    public IEndpointRecorder AddInitial(
        string route,
        ExchangerDelegate exchangerDelegate,
        PackageSchema? requestSchemaPatch = default,
        PackageSchema? responseSchemaPatch = default)
    {
        AddEndpoint(
            new Route(route),
            InitialDeliveryMethod,
            exchangerDelegate,
            EndpointType.Exchanger,
            isInitial: true,
            requestSchemaPatch,
            responseSchemaPatch);
        return this;
    }

    public IEndpointRecorder AddInitial(
        Route route,
        ExchangerDelegate exchangerDelegate,
        PackageSchema? requestSchemaPatch = default,
        PackageSchema? responseSchemaPatch = default)
    {
        AddEndpoint(
            route,
            InitialDeliveryMethod,
            exchangerDelegate,
            EndpointType.Exchanger,
            isInitial: true,
            requestSchemaPatch,
            responseSchemaPatch);
        return this;
    }

    private void AddEndpoint(
        Route route,
        DeliveryMethod deliveryMethod,
        Delegate endpointDelegate,
        EndpointType endpointType,
        bool isInitial = false,
        PackageSchema? requestSchemaPatch = default,
        PackageSchema? responseSchemaPatch = default)
    {
        if (route is null) throw new ArgumentNullException(nameof(route));
        if (endpointDelegate is null) throw new ArgumentNullException(nameof(endpointDelegate));

        if (_endpointsStorage.LocalEndpoints.Any(_ => _.EndpointData.Route.Equals(route)))
            throw new FatNetLibException($"Endpoint with the route : {route} was already registered");

        var endpoint = new Endpoint(
            route,
            endpointType,
            deliveryMethod,
            isInitial,
            requestSchemaPatch ?? new PackageSchema(),
            responseSchemaPatch ?? new PackageSchema());

        var localEndpoint = new LocalEndpoint(endpoint, endpointDelegate);
        _endpointsStorage.LocalEndpoints.Add(localEndpoint);
    }

    public IEndpointRecorder AddController(IController controller)
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
             CreateLocalEndpointFromMethod(method, controller, mainRoute, isInitial);
        }

        return this;
    }

    private void CreateLocalEndpointFromMethod(
        MethodInfo method,
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
                    endpointType = EndpointType.Exchanger;
                    deliveryMethod = exchanger.DeliveryMethod;
                    break;
                }
            }
        }

        if (methodRoute.IsEmpty)
            throw new FatNetLibException(
                $"{method.Name} in {controller.GetType().Name} does not have route attribute");

        if (isInitial)
        {
            endpointType = EndpointType.Exchanger;
            deliveryMethod = InitialDeliveryMethod;
        }

        if (endpointType is null)
            throw new FatNetLibException(
                $"{method.Name} in {controller.GetType().Name} does not have endpoint type attribute");

        Route fullRoute = mainRoute + methodRoute;

        PackageSchema requestSchemaPatch = CreateRequestSchemaPatch(method);
        PackageSchema responseSchemaPatch = CreateResponseSchemaPatch(method);
        Delegate methodDelegate = CreateDelegateFromMethod(method, controller);
        AddEndpoint(
            fullRoute,
            deliveryMethod!.Value,
            methodDelegate,
            endpointType.Value,
            isInitial,
            requestSchemaPatch,
            responseSchemaPatch);
    }

    private static PackageSchema CreateRequestSchemaPatch(MethodInfo method)
    {
        return CreateSchemaPatch(method.GetCustomAttributes<SchemaAttribute>());
    }

    private static PackageSchema CreateResponseSchemaPatch(MethodInfo method)
    {
        return CreateSchemaPatch(method.ReturnParameter.GetCustomAttributes<SchemaAttribute>());
    }

    private static PackageSchema CreateSchemaPatch(IEnumerable<SchemaAttribute> schemaAttributes)
    {
        var patch = new PackageSchema();
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
