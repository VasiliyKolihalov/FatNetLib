using System;
using System.Collections.Generic;

namespace Kolyhalov.FatNetLib.Core.Middlewares
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
