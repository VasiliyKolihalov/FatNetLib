using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Kolyhalov.FatNetLib.Core.Attributes;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Controllers;
using Kolyhalov.FatNetLib.Core.Delegates;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Storages;

namespace Kolyhalov.FatNetLib.Core.Recorders
{
    public class EndpointRecorder : IEndpointRecorder
    {
        private const BindingFlags EndpointSearch =
            BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public;

        private const Reliability InitialReliability = Reliability.ReliableOrdered;

        private readonly IEndpointsStorage _endpointsStorage;

        public EndpointRecorder(IEndpointsStorage endpointsStorage)
        {
            _endpointsStorage = endpointsStorage;
        }

        public IEndpointRecorder AddReceiver(
            Route route,
            ReceiverDelegate receiverDelegate,
            Reliability reliability = Reliability.ReliableOrdered,
            PackageSchema? requestSchemaPatch = default)
        {
            AddEndpoint(
                route,
                reliability,
                receiverDelegate,
                EndpointType.Receiver,
                requestSchemaPatch: requestSchemaPatch);
            return this;
        }

        public IEndpointRecorder AddExchanger(
            Route route,
            ExchangerDelegate exchangerDelegate,
            Reliability reliability = Reliability.ReliableOrdered,
            PackageSchema? requestSchemaPatch = default,
            PackageSchema? responseSchemaPatch = default)
        {
            AddEndpoint(
                route,
                reliability,
                exchangerDelegate,
                EndpointType.Exchanger,
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
                InitialReliability,
                exchangerDelegate,
                EndpointType.Initial,
                requestSchemaPatch,
                responseSchemaPatch);
            return this;
        }

        public IEndpointRecorder AddEvent(Route route, EventDelegate eventDelegate)
        {
            if (route is null) throw new ArgumentNullException(nameof(route));
            if (eventDelegate is null) throw new ArgumentNullException(nameof(eventDelegate));

            var endpoint = new Endpoint(
                route,
                EndpointType.Event,
                InitialReliability,
                requestSchemaPatch: new PackageSchema(),
                responseSchemaPatch: new PackageSchema());

            var localEndpoint = new LocalEndpoint(endpoint, eventDelegate);
            _endpointsStorage.LocalEndpoints.Add(localEndpoint);

            return this;
        }

        private void AddEndpoint(
            Route route,
            Reliability reliability,
            Delegate endpointDelegate,
            EndpointType endpointType,
            PackageSchema? requestSchemaPatch = default,
            PackageSchema? responseSchemaPatch = default)
        {
            if (route is null) throw new ArgumentNullException(nameof(route));
            if (endpointDelegate is null) throw new ArgumentNullException(nameof(endpointDelegate));

            if (_endpointsStorage.LocalEndpoints.Any(_ => _.EndpointData.Route.Equals(route)))
                throw new FatNetLibException($"Endpoint with the route {route} was already registered");

            var endpoint = new Endpoint(
                route,
                endpointType,
                reliability,
                requestSchemaPatch ?? new PackageSchema(),
                responseSchemaPatch ?? new PackageSchema());

            var localEndpoint = new LocalEndpoint(endpoint, endpointDelegate);
            _endpointsStorage.LocalEndpoints.Add(localEndpoint);
        }

        public IEndpointRecorder AddController(IController controller)
        {
            if (controller == null) throw new ArgumentNullException(nameof(controller));

            Type controllerType = controller.GetType();
            object[] controllerAttributes = controllerType.GetCustomAttributes(inherit: false);
            var mainRoute = Route.Empty;
            foreach (object attribute in controllerAttributes)
            {
                switch (attribute)
                {
                    case RouteAttribute route:
                        mainRoute += route.Route;
                        break;
                }
            }

            foreach (MethodInfo method in controllerType.GetMethods(EndpointSearch))
            {
                CreateLocalEndpointFromMethod(method, controller, mainRoute);
            }

            return this;
        }

        private void CreateLocalEndpointFromMethod(
            MethodInfo method,
            IController controller,
            Route mainRoute)
        {
            object[] methodAttributes = method.GetCustomAttributes(inherit: true);
            var methodRoute = Route.Empty;
            EndpointType? endpointType = null;
            Reliability? reliability = null;
            foreach (object attribute in methodAttributes)
            {
                switch (attribute)
                {
                    case RouteAttribute routeAttribute:
                        methodRoute += routeAttribute.Route;
                        break;

                    case ReceiverAttribute receiver:
                        endpointType = EndpointType.Receiver;
                        reliability = receiver.Reliability;
                        break;

                    case ExchangerAttribute exchanger:
                    {
                        endpointType = EndpointType.Exchanger;
                        reliability = exchanger.Reliability;
                        break;
                    }

                    case InitialAttribute _:
                    {
                        endpointType = EndpointType.Initial;
                        reliability = InitialReliability;
                        break;
                    }

                    case EventAttribute _:
                    {
                        endpointType = EndpointType.Event;
                        reliability = InitialReliability;
                        break;
                    }
                }
            }

            if (methodRoute.IsEmpty)
                throw new FatNetLibException(
                    $"{method.Name} in {controller.GetType().Name} does not have route attribute");

            if (endpointType is null)
                throw new FatNetLibException(
                    $"{method.Name} in {controller.GetType().Name} does not have endpoint type attribute");

            Route fullRoute = mainRoute + methodRoute;

            PackageSchema requestSchemaPatch = CreateRequestSchemaPatch(method);
            PackageSchema responseSchemaPatch = CreateResponseSchemaPatch(method);
            Delegate methodDelegate = CreateDelegateFromMethod(method, controller);
            AddEndpoint(
                fullRoute,
                reliability!.Value,
                methodDelegate,
                endpointType.Value,
                requestSchemaPatch,
                responseSchemaPatch);
        }

        private static PackageSchema CreateRequestSchemaPatch(MethodInfo method)
        {
            return CreateSchemaPatch(method.GetCustomAttributes<SchemaAttribute>());
        }

        private static PackageSchema CreateResponseSchemaPatch(MethodInfo method)
        {
            return CreateSchemaPatch(method.ReturnParameter!.GetCustomAttributes<SchemaAttribute>());
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
}
