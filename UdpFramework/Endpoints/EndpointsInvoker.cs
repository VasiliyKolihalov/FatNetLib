using System.Reflection;
using Newtonsoft.Json;

namespace Kolyhalov.UdpFramework.Endpoints;

public class EndpointsInvoker : IEndpointsInvoker
{
    public Package? InvokeEndpoint(LocalEndpoint endpoint, Package package)
    {
        object[] arguments = GetEndpointArgumentsFromPackage(endpoint, package);
        object? target = endpoint.MethodDelegate.Target;
        object? responsePackage;
        try
        {
           responsePackage = endpoint.MethodDelegate.Method.Invoke(target, arguments);
        }
        catch (TargetInvocationException exception)
        {
            throw new UdpFrameworkException(exception.GetBaseException().Message);
        }
        
        if (endpoint.EndpointData.EndpointType == EndpointType.Exchanger)
        {
            return (Package) responsePackage!;
        }
        return null;
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
            catch(JsonSerializationException)
            {
                throw new UdpFrameworkException($"Failed to deserialize package field to parameter: {parameter.Name}");
            }
        }

        return arguments.ToArray();
    }
}