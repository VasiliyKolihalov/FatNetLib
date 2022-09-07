using System.Reflection;
using Newtonsoft.Json;

namespace Kolyhalov.UdpFramework.Endpoints;

public class EndpointsInvoker : IEndpointsInvoker
{
    public void InvokeReceiver(LocalEndpoint endpoint, Package requestPackage)
    {
        InvokeEndpoint(endpoint, requestPackage);
    }

    public Package InvokeExchanger(LocalEndpoint endpoint, Package requestPackage)
    {
        Package responsePackage = InvokeEndpoint(endpoint, requestPackage) ??
               throw new UdpFrameworkException("Exchanger cannot return null");
        
        if (responsePackage.Route != null && responsePackage.Route != requestPackage.Route)
        {
            throw new UdpFrameworkException("Pointing response packages to another route is not allowed");
        }

        if (responsePackage.ExchangeId != null && responsePackage.ExchangeId != requestPackage.ExchangeId)
        {
            throw new UdpFrameworkException("Changing response exchangeId to another is not allowed");
        }

        return responsePackage;
    }

    private Package? InvokeEndpoint(LocalEndpoint endpoint, Package package)
    {
        object[] arguments = GetEndpointArgumentsFromPackage(endpoint, package);
        object? target = endpoint.MethodDelegate.Target;
        Package? responsePackage;
        try
        {
            responsePackage = (Package?) endpoint.MethodDelegate.Method.Invoke(target, arguments);
        }
        catch (TargetInvocationException exception)
        {
            throw new UdpFrameworkException("Endpoint invocation failed", exception);
        }

        return endpoint.EndpointData.EndpointType == EndpointType.Exchanger ? responsePackage : null;
    }

    private static object[] GetEndpointArgumentsFromPackage(LocalEndpoint endpoint, Package package)
    {
        var arguments = new List<object>();

        foreach (ParameterInfo parameter in endpoint.Parameters)
        {
            if (parameter.ParameterType == typeof(Package))
            {
                arguments.Add(package);
                continue;
            }

            if (package.Body == null)
                throw new UdpFrameworkException("Package body is null");

            string parameterName = parameter.Name!.Substring(0, 1).ToUpper() + parameter.Name!.Remove(0, 1);
            try
            {
                string bodyField = package.Body[parameterName].ToString()!;
                arguments.Add(JsonConvert.DeserializeObject(bodyField, parameter.ParameterType)!);
            }
            catch (KeyNotFoundException)
            {
                throw new UdpFrameworkException($"There is no required field: {parameter.Name} in the package");
            }
            catch (JsonSerializationException exception)
            {
                throw new UdpFrameworkException($"Failed to deserialize package field to parameter: {parameter.Name}",
                    exception);
            }
        }

        return arguments.ToArray();
    }
}