using System;
using Kolyhalov.FatNetLib.Core.Models;

namespace Kolyhalov.FatNetLib.Core.Utils
{
    public static class ExceptionUtils
    {
        public static EndpointRunFailedView ToEndpointRunFailedView(this Exception exception) =>
            new EndpointRunFailedView
            {
                Message = exception.Message,
                Type = exception.GetType(),
                InnerExceptionView = exception.InnerException?.ToEndpointRunFailedView() ?? null
            };
    }
}
