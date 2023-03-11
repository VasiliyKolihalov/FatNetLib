using System;
using System.Collections.Generic;
using System.Reflection;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Utils;

namespace Kolyhalov.FatNetLib.Core
{
    public class EndpointsInvoker : IEndpointsInvoker
    {
        private readonly ILogger _logger;

        public EndpointsInvoker(ILogger logger)
        {
            _logger = logger;
        }

        public void InvokeReceiver(LocalEndpoint endpoint, Package requestPackage)
        {
            InvokeEndpoint(endpoint, requestPackage);
        }

        public Package InvokeExchanger(LocalEndpoint endpoint, Package requestPackage)
        {
            Package responsePackage = InvokeEndpoint(endpoint, requestPackage) ??
                                      throw new FatNetLibException("Exchanger returned null which is not allowed. + " +
                                                                   $"Endpoint route: {endpoint.Details.Route}");

            if (responsePackage.Fields.ContainsKey(nameof(Package.Route))
                && responsePackage.Route!.NotEquals(requestPackage.Route))
            {
                throw new FatNetLibException("Changing response Route to another is not allowed. " +
                                             $"Endpoint route: {endpoint.Details.Route}");
            }

            if (responsePackage.Fields.ContainsKey(nameof(Package.ExchangeId))
                && responsePackage.ExchangeId != requestPackage.ExchangeId)
            {
                throw new FatNetLibException("Changing response ExchangeId to another is not allowed. " +
                                             $"Endpoint route: {endpoint.Details.Route}");
            }

            if (responsePackage.Fields.ContainsKey(nameof(Package.IsResponse))
                && !responsePackage.IsResponse)
            {
                throw new FatNetLibException("Changing response IsResponse to another is not allowed. " +
                                             $"Endpoint route: {endpoint.Details.Route}");
            }

            return responsePackage;
        }

        private Package? InvokeEndpoint(LocalEndpoint endpoint, Package package)
        {
            object? target = endpoint.Action.Target;
            object[] arguments = GetEndpointArgumentsFromPackage(endpoint, package);
            try
            {
                // Todo: pass peer id to the method
                // Todo: wrap the delegate and test passed arguments correctly
                return (Package?)endpoint.Action.Method.Invoke(target, arguments);
            }
            catch (TargetInvocationException invocationException)
            {
                Exception causeException =
                    invocationException.InnerException
                    ?? throw new FatNetLibException(
                        $"Endpoint invocation failed for unknown reason. Endpoint route {endpoint.Details.Route}",
                        invocationException);

                _logger.Error(causeException, $"Endpoint invocation failed. Endpoint route {endpoint.Details.Route}");
                return new Package { NonSendingFields = { ["InvocationException"] = causeException } };
            }
        }

        private static object[] GetEndpointArgumentsFromPackage(LocalEndpoint endpoint, Package package)
        {
            var arguments = new List<object>();

            foreach (ParameterInfo parameter in endpoint.Parameters)
            {
                if (parameter.ParameterType != typeof(Package))
                    throw new FatNetLibException($"Cannot provide parameter {parameter.Name}. " +
                                                 $"Endpoint route {endpoint.Details.Route}. " +
                                                 "Only one parameter of type package is allowed");

                arguments.Add(package);
            }

            return arguments.ToArray();
        }
    }
}
