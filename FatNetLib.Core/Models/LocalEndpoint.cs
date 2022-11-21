﻿using System;
using System.Reflection;
using Kolyhalov.FatNetLib.Core.Exceptions;

namespace Kolyhalov.FatNetLib.Core.Models
{
    public class LocalEndpoint
    {
        public Endpoint EndpointData { get; }

        public Delegate MethodDelegate { get; }

        public ParameterInfo[] Parameters { get; }

        public LocalEndpoint(Endpoint endpoint, Delegate methodDelegate)
        {
            EndpointData = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            MethodDelegate = methodDelegate ?? throw new ArgumentNullException(nameof(methodDelegate));
            if ((EndpointData.EndpointType is EndpointType.Exchanger ||
                 EndpointData.EndpointType is EndpointType.Initial) &&
                methodDelegate.Method.ReturnType != typeof(Package))
                throw new FatNetLibException("Return type of exchanger and initial should be Package");
            Parameters = MethodDelegate.Method.GetParameters();
        }
    }
}
