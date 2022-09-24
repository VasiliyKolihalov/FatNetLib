using System.Reflection;
using Newtonsoft.Json;

namespace Kolyhalov.FatNetLib.Endpoints;

public class EndpointsInvoker : IEndpointsInvoker
{
    public void InvokeReceiver(LocalEndpoint endpoint, Package requestPackage)
    {
        InvokeEndpoint(endpoint, requestPackage);
    }

    public Package InvokeExchanger(LocalEndpoint endpoint, Package requestPackage)
    {
        Package responsePackage = InvokeEndpoint(endpoint, requestPackage) ??
               throw new FatNetLibException("Exchanger cannot return null");
        
        if (responsePackage.Route != null && responsePackage.Route != requestPackage.Route)
        {
            throw new FatNetLibException("Pointing response packages to another route is not allowed");
        }

        if (responsePackage.ExchangeId != Guid.Empty && responsePackage.ExchangeId != requestPackage.ExchangeId)
        {
            throw new FatNetLibException("Changing response exchangeId to another is not allowed");
        }

        return responsePackage;
    }

    private static Package? InvokeEndpoint(LocalEndpoint endpoint, Package package)
    {
        object[] arguments = GetEndpointArgumentsFromPackage(endpoint, package);
        object? target = endpoint.MethodDelegate.Target;
        Package? responsePackage;
        try
        {
            // todo: pass peer id to the method
            responsePackage = (Package?) endpoint.MethodDelegate.Method.Invoke(target, arguments);
        }
        catch (TargetInvocationException exception)
        {
            throw new FatNetLibException("Endpoint invocation failed", exception);
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
                throw new FatNetLibException("Package body is null");

            string parameterName = parameter.Name!.Substring(0, 1).ToUpper() + parameter.Name!.Remove(0, 1);
            try
            {
                var bodyField = package.Body[parameterName].ToString()!;
                arguments.Add(JsonConvert.DeserializeObject(bodyField, parameter.ParameterType)!);
            }
            catch (KeyNotFoundException)
            {
                throw new FatNetLibException($"There is no required field: {parameter.Name} in the package");
            }
            catch (JsonSerializationException exception)
            {
                throw new FatNetLibException($"Failed to deserialize package field to parameter: {parameter.Name}",
                    exception);
            }
        }

        return arguments.ToArray();
    }
}