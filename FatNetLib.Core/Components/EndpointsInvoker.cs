using System;
using System.Reflection;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Utils;

namespace Kolyhalov.FatNetLib.Core.Components
{
    public class EndpointsInvoker : IEndpointsInvoker
    {
        private readonly IControllerArgumentsExtractor _argumentsExtractor;
        private readonly ILogger _logger;

        public EndpointsInvoker(IControllerArgumentsExtractor argumentsExtractor, ILogger logger)
        {
            _argumentsExtractor = argumentsExtractor;
            _logger = logger;
        }

        public void InvokeConsumer(LocalEndpoint endpoint, Package requestPackage)
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
            object?[] arguments = _argumentsExtractor.ExtractFromPackage(package, endpoint);
            try
            {
                // Todo: wrap the delegate and test passed arguments correctly
                object result = endpoint.Action.Method.Invoke(target, arguments);
                if (result is Package || result is null)
                    return (Package?)result;

                return new Package
                {
                    Body = result
                };
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
    }
}
