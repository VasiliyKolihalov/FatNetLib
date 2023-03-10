using System;
using System.Reflection;
using Kolyhalov.FatNetLib.Core.Exceptions;

namespace Kolyhalov.FatNetLib.Core.Models
{
    public class LocalEndpoint
    {
        public Endpoint Details { get; }

        public Delegate Action { get; }

        public ParameterInfo[] Parameters { get; }

        public LocalEndpoint(Endpoint endpoint, Delegate action)
        {
            Details = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            Action = action ?? throw new ArgumentNullException(nameof(action));
            if ((Details.Type is EndpointType.Exchanger || Details.Type is EndpointType.Initializer)
                && action.Method.ReturnType != typeof(Package))
                throw new FatNetLibException(
                    $"Return type of exchanger or initial endpoint should be Package. Endpoint route: {Details.Route}");
            Parameters = Action.Method.GetParameters();
        }
    }
}
