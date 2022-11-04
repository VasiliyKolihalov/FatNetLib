using System.Reflection;

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

        if (responsePackage.Route is not null && !responsePackage.Route.Equals(requestPackage.Route))
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
        object? target = endpoint.MethodDelegate.Target;
        object[] arguments = GetEndpointArgumentsFromPackage(endpoint, package);
        try
        {
            // Todo: pass peer id to the method
            // Todo: wrap the delegate and test passed arguments correctly
            return (Package?)endpoint.MethodDelegate.Method.Invoke(target, arguments);
        }
        catch (TargetInvocationException exception)
        {
            throw new FatNetLibException("Endpoint invocation failed", exception);
        }
    }

    private static object[] GetEndpointArgumentsFromPackage(LocalEndpoint endpoint, Package package)
    {
        var arguments = new List<object>();

        foreach (ParameterInfo parameter in endpoint.Parameters)
        {
            if (parameter.ParameterType != typeof(Package))
                throw new FatNetLibException($"Cannot provide parameter {parameter.Name}");

            arguments.Add(package);
        }

        return arguments.ToArray();
    }
}
