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

        public IEndpointRecorder AddConsumer(
            Route route,
            ConsumerAction action,
            Reliability reliability = Reliability.ReliableOrdered,
            PackageSchema? requestSchemaPatch = default)
        {
            AddNetworkEndpoint(
                route,
                reliability,
                action,
                EndpointType.Consumer,
                requestSchemaPatch: requestSchemaPatch);
            return this;
        }

        public IEndpointRecorder AddConsumer(
            Route route,
            Delegate action,
            Reliability reliability = Reliability.ReliableOrdered,
            PackageSchema? requestSchemaPatch = default)
        {
            AddNetworkEndpoint(
                route,
                reliability,
                action,
                EndpointType.Consumer,
                requestSchemaPatch: requestSchemaPatch);
            return this;
        }

        public IEndpointRecorder AddExchanger(
            Route route,
            ExchangerAction action,
            Reliability reliability = Reliability.ReliableOrdered,
            PackageSchema? requestSchemaPatch = default,
            PackageSchema? responseSchemaPatch = default)
        {
            AddNetworkEndpoint(
                route,
                reliability,
                action,
                EndpointType.Exchanger,
                requestSchemaPatch,
                responseSchemaPatch);
            return this;
        }

        public IEndpointRecorder AddExchanger(
            Route route,
            Delegate action,
            Reliability reliability = Reliability.ReliableOrdered,
            PackageSchema? requestSchemaPatch = default,
            PackageSchema? responseSchemaPatch = default)
        {
            AddNetworkEndpoint(
                route,
                reliability,
                action,
                EndpointType.Exchanger,
                requestSchemaPatch,
                responseSchemaPatch);
            return this;
        }

        public IEndpointRecorder AddInitial(
            Route route,
            ExchangerAction action,
            PackageSchema? requestSchemaPatch = default,
            PackageSchema? responseSchemaPatch = default)
        {
            AddNetworkEndpoint(
                route,
                InitialReliability,
                action,
                EndpointType.Initializer,
                requestSchemaPatch,
                responseSchemaPatch);
            return this;
        }

        public IEndpointRecorder AddInitial(
            Route route,
            Delegate action,
            PackageSchema? requestSchemaPatch = default,
            PackageSchema? responseSchemaPatch = default)
        {
            AddNetworkEndpoint(
                route,
                InitialReliability,
                action,
                EndpointType.Initializer,
                requestSchemaPatch,
                responseSchemaPatch);
            return this;
        }

        public IEndpointRecorder AddEventListener(Route route, EventAction action)
        {
            return AddEventListener(route, (Delegate)action);
        }

        public IEndpointRecorder AddEventListener(Route route, Delegate action)
        {
            if (route is null) throw new ArgumentNullException(nameof(route));
            if (action is null) throw new ArgumentNullException(nameof(action));

            var endpoint = new Endpoint(
                route,
                EndpointType.EventListener,
                InitialReliability,
                requestSchemaPatch: new PackageSchema(),
                responseSchemaPatch: new PackageSchema());

            var localEndpoint = new LocalEndpoint(endpoint, action);
            _endpointsStorage.LocalEndpoints.Add(localEndpoint);

            return this;
        }

        private void AddNetworkEndpoint(
            Route route,
            Reliability reliability,
            Delegate endpointDelegate,
            EndpointType endpointType,
            PackageSchema? requestSchemaPatch = default,
            PackageSchema? responseSchemaPatch = default)
        {
            if (route is null) throw new ArgumentNullException(nameof(route));
            if (endpointDelegate is null) throw new ArgumentNullException(nameof(endpointDelegate));

            if (_endpointsStorage.LocalEndpoints.Any(_ => _.Details.Route.Equals(route)))
                throw new FatNetLibException($"Endpoint with the route {route} was already registered");

            var endpoint = new Endpoint(
                route,
                endpointType,
                reliability,
                requestSchemaPatch ?? CreateSchemaPatchFromParameterAttributes(endpointDelegate.Method),
                responseSchemaPatch ?? CreateSchemaPatchFromReturnType(endpointDelegate.Method));

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

                    case ConsumerAttribute consumer:
                        endpointType = EndpointType.Consumer;
                        reliability = consumer.Reliability;
                        break;

                    case ExchangerAttribute exchanger:
                    {
                        endpointType = EndpointType.Exchanger;
                        reliability = exchanger.Reliability;
                        break;
                    }

                    case InitializerAttribute _:
                    {
                        endpointType = EndpointType.Initializer;
                        reliability = InitialReliability;
                        break;
                    }

                    case EventListenerAttribute _:
                    {
                        endpointType = EndpointType.EventListener;
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
            Delegate action = CreateActionFromMethod(method, controller);
            AddNetworkEndpoint(
                fullRoute,
                reliability!.Value,
                action,
                endpointType.Value,
                requestSchemaPatch,
                responseSchemaPatch);
        }

        private static PackageSchema CreateRequestSchemaPatch(MethodInfo method)
        {
            PackageSchema schemaFromParameterAttributes = CreateSchemaPatchFromParameterAttributes(method);
            PackageSchema schemaFromMethodAttributes =
                CreateSchemaPatchFromMethodAttributes(method.GetCustomAttributes<SchemaAttribute>());

            schemaFromParameterAttributes.Patch(schemaFromMethodAttributes);

            return schemaFromParameterAttributes;
        }

        private static PackageSchema CreateSchemaPatchFromParameterAttributes(MethodInfo method)
        {
            var patch = new PackageSchema();

            foreach (ParameterInfo parameterInfo in method.GetParameters())
            {
                foreach (Attribute attribute in parameterInfo.GetCustomAttributes())
                {
                    switch (attribute)
                    {
                        case BodyAttribute _:
                            patch.Add(nameof(Package.Body), parameterInfo.ParameterType);
                            continue;
                        case ErrorAttribute _:
                            patch.Add(nameof(Package.Error), parameterInfo.ParameterType);
                            continue;
                        case Field field:
                            patch.Add(field.Name, parameterInfo.ParameterType);
                            continue;
                    }
                }
            }

            return patch;
        }

        private static PackageSchema CreateResponseSchemaPatch(MethodInfo method)
        {
            PackageSchema schemaFromReturnType = CreateSchemaPatchFromReturnType(method);

            PackageSchema schemaFromMethodAttributes = CreateSchemaPatchFromMethodAttributes(method.ReturnParameter!
                .GetCustomAttributes<SchemaAttribute>());

            schemaFromReturnType.Patch(schemaFromMethodAttributes);

            return schemaFromReturnType;
        }

        private static PackageSchema CreateSchemaPatchFromReturnType(MethodInfo method)
        {
            var schema = new PackageSchema();

            if (method.ReturnType != typeof(Package) && method.ReturnType != typeof(void))
                schema.Add(nameof(Package.Body), method.ReturnType);

            return schema;
        }

        private static PackageSchema CreateSchemaPatchFromMethodAttributes(
            IEnumerable<SchemaAttribute> schemaAttributes)
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

        private static Delegate CreateActionFromMethod(MethodInfo methodInfo, IController controller)
        {
            IEnumerable<Type> paramTypes = methodInfo.GetParameters().Select(parameter => parameter.ParameterType);
            Type delegateType = Expression.GetDelegateType(paramTypes.Append(methodInfo.ReturnType).ToArray());
            return methodInfo.CreateDelegate(delegateType, controller);
        }
    }
}
