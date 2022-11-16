using System;
using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Middlewares;
using Kolyhalov.FatNetLib.Core.Models;

namespace Kolyhalov.FatNetLib.Core.Runners
{
    public class MiddlewaresRunner : IMiddlewaresRunner
    {
        private readonly IEnumerable<IMiddleware> _middlewares;

        public MiddlewaresRunner(IEnumerable<IMiddleware> middlewares)
        {
            _middlewares = middlewares;
        }

        public void Process(Package package)
        {
            foreach (IMiddleware middleware in _middlewares)
            {
                try
                {
                    middleware.Process(package);
                }
                catch (Exception exception)
                {
                    throw new FatNetLibException($"Middleware \"{middleware.GetType().Name}\" failed", exception);
                }
            }
        }
    }
}
