using System.Linq;
using System.Reflection;
using Kolyhalov.FatNetLib.Core.Attributes;
using Kolyhalov.FatNetLib.Core.Couriers;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Storages;

namespace Kolyhalov.FatNetLib.Core.Components
{
    public class ControllerArgumentsExtractor : IControllerArgumentsExtractor
    {
        private static readonly object NullArgument = new object();

        public object?[] ExtractFromPackage(Package package, LocalEndpoint endpoint)
        {
            return endpoint.Parameters
                .Select(parameter => GetEndpointArgument(parameter, package, endpoint))
                .ToArray();
        }

        private static object? GetEndpointArgument(ParameterInfo parameter, Package package, LocalEndpoint endpoint)
        {
            object? argument = GetPackageArgument(package, parameter)
                               ?? GetCourierArgument(package, parameter)
                               ?? GetContextArgument(package, parameter)
                               ?? GetBodyArgument(package, parameter)
                               ?? GetErrorArgument(package, parameter)
                               ?? GetSenderArgument(package, parameter)
                               ?? GetReceiverArgument(package, parameter)
                               ?? GetExchangeIdArgument(package, parameter)
                               ?? GetFromPackageArgument(package, parameter);
            if (argument == null)
                throw new FatNetLibException($"Cannot provide parameter {parameter.Name}. " +
                                             $"Endpoint route {endpoint.Details.Route}.");
            return argument == NullArgument ? null : argument;
        }

        private static void CheckArgumentType(object? argument, ParameterInfo parameter)
        {
            if (argument != null && argument.GetType().IsInstanceOfType(parameter.ParameterType))
                throw new FatNetLibException($"Cannot provide {parameter.Name} because of type mismatch");
        }

        private static object? GetPackageArgument(Package package, ParameterInfo parameter)
        {
            return parameter.ParameterType == typeof(Package)
                ? package
                : null;
        }

        private static object? GetCourierArgument(Package package, ParameterInfo parameter)
        {
            return parameter.ParameterType == typeof(ICourier)
                ? package.Courier ?? NullArgument
                : null;
        }

        private static object? GetContextArgument(Package package, ParameterInfo parameter)
        {
            return parameter.ParameterType == typeof(IDependencyContext)
                ? package.Context ?? NullArgument
                : null;
        }

        private static object? GetBodyArgument(Package package, ParameterInfo parameter)
        {
            if (parameter.GetCustomAttribute<BodyAttribute>() == null) return null;
            CheckArgumentType(package.Body, parameter);
            return package.Body ?? NullArgument;
        }

        private static object? GetErrorArgument(Package package, ParameterInfo parameter)
        {
            if (parameter.GetCustomAttribute<ErrorAttribute>() == null) return null;
            CheckArgumentType(package.Error, parameter);
            return package.Error ?? NullArgument;
        }

        private static object? GetSenderArgument(Package package, ParameterInfo parameter)
        {
            if (parameter.GetCustomAttribute<Sender>() == null) return null;
            CheckArgumentType(package.Sender, parameter);
            return package.Sender ?? NullArgument;
        }

        private static object? GetReceiverArgument(Package package, ParameterInfo parameter)
        {
            if (parameter.GetCustomAttribute<ReceiverAttribute>() == null) return null;
            CheckArgumentType(package.Receiver, parameter);
            return package.Receiver ?? NullArgument;
        }

        private static object? GetExchangeIdArgument(Package package, ParameterInfo parameter)
        {
            if (parameter.GetCustomAttribute<ExchangeIdAttribute>() == null) return null;
            CheckArgumentType(package.ExchangeId, parameter);
            return package.ExchangeId ?? NullArgument;
        }

        private static object? GetFromPackageArgument(Package package, ParameterInfo parameter)
        {
            if (parameter.GetCustomAttribute<FromPackageAttribute>() == null) return null;
            var argument = package.GetAnyField<object?>(parameter.GetCustomAttribute<FromPackageAttribute>().Field);
            CheckArgumentType(argument, parameter);
            return argument ?? NullArgument;
        }
    }
}
