using System;
using System.Reflection;
using Kolyhalov.FatNetLib.Core.Exceptions;

namespace Kolyhalov.FatNetLib.Core.Models
{
    public class LocalEndpoint
    {
        public Endpoint EndpointData { get; }

        public Delegate Action { get; }

        public ParameterInfo[] Parameters { get; }

        public LocalEndpoint(Endpoint endpoint, Delegate action)
        {
            EndpointData = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            Action = action ?? throw new ArgumentNullException(nameof(action));
            if ((EndpointData.EndpointType is EndpointType.Exchanger ||
                 EndpointData.EndpointType is EndpointType.Initializer) &&
                action.Method.ReturnType != typeof(Package))
                throw new FatNetLibException("Return type of exchanger and initial should be Package");
            Parameters = Action.Method.GetParameters();
        }
    }
}
