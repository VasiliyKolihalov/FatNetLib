using Newtonsoft.Json;

namespace Kolyhalov.UdpFramework;

public class EndpointsInvoker : IEndpointsInvoker
{
    public Package? InvokeEndpoint(LocalEndpoint endpoint, Package package)
    {
        object[] arguments = GetEndpointArgumentsFromPackage(endpoint, package);
        object? target = endpoint.MethodDelegate.Target;
        object? responsePackage = endpoint.MethodDelegate.Method.Invoke(target, arguments);

        if (endpoint.EndpointData.EndpointType == EndpointType.Exchanger)
        {
            return (Package) responsePackage!;
        }

        return null;
    }

    private static object[] GetEndpointArgumentsFromPackage(LocalEndpoint endpoint, Package package)
    {
        List<object> arguments = new List<object>();

        foreach (var parameterType in endpoint.ParameterTypes)
        {
            if (parameterType == typeof(Package))
            {
                arguments.Add(package);
                continue;
            }
            
            if (package.Body == null)
                throw new UdpFrameworkException("Package body is null");
            
            try
            {
                string bodyField = package.Body[parameterType.Name].ToString()!;
                arguments.Add(JsonConvert.DeserializeObject(bodyField, parameterType)!);
            }
            catch
            {
                throw new UdpFrameworkException($"There is no required field: {parameterType.Name} in the package");
            }
        }

        return arguments.ToArray();
    }
}