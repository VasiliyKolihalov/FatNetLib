using System.Reflection;
using Newtonsoft.Json;
using static Kolyhalov.UdpFramework.UdpFramework;

namespace Kolyhalov.UdpFramework;

public class EndpointsInvoker : IEndpointsInvoker
{
    public Package? InvokeEndpoint(LocalEndpoint endpoint, Package package)
    {
        object[] arguments = GetEndpointArgumentsFromPackage(endpoint, package);

        if (endpoint.EndpointData.EndpointType == EndpointType.Exchanger)
        {
            Package responsePackage;
            if (endpoint.Controller == null)
            {
                ExchangerDelegate exchangerDelegate = CreateDelegateFromMethodInfo<ExchangerDelegate>(endpoint.Method);
                responsePackage = exchangerDelegate.Invoke(package);
            }
            else
            {
                responsePackage = (Package) endpoint.Method.Invoke(endpoint.Controller, arguments)!;
            }

            return responsePackage;
        }
        else
        {
            if (endpoint.Controller == null)
            {
                ReceiverDelegate receiverDelegate = CreateDelegateFromMethodInfo<ReceiverDelegate>(endpoint.Method);
                receiverDelegate.Invoke(package);
                return null;
            }

            endpoint.Method.Invoke(endpoint.Controller, arguments);
            return null;
        }
    }

    private static object[] GetEndpointArgumentsFromPackage(LocalEndpoint endpoint, Package package)
    {
        List<Type> parameterTypes = new List<Type>();
        foreach (var parameterType in endpoint.Method.GetParameters())
        {
            parameterTypes.Add(parameterType.ParameterType);
        }

        List<object> arguments = new List<object>();

        foreach (var parameterType in parameterTypes)
        {
            try
            {
                if (endpoint.Controller == null)
                {
                    arguments.Add(package);
                    break;
                }

                arguments.Add(JsonConvert.DeserializeObject(package.Body[parameterType.Name].ToString()!,
                    parameterType)!);
            }
            catch
            {
                throw new UdpFrameworkException($"There is no required field: {parameterType.Name} in the package");
            }
        }

        return arguments.ToArray();
    }

    private static T CreateDelegateFromMethodInfo<T>(MethodInfo methodInfo) where T : Delegate
    {
        return (T) Delegate.CreateDelegate(typeof(ReceiverDelegate), firstArgument:null, methodInfo);
    }
}