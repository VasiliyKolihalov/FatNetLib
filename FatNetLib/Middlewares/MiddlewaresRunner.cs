﻿namespace Kolyhalov.FatNetLib.Middlewares;

public class MiddlewaresRunner : IMiddlewaresRunner
{
    private readonly IList<IMiddleware> _middlewares;

    public MiddlewaresRunner(IList<IMiddleware> middlewares)
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
                throw new FatNetLibException($"Middleware \"{nameof(middleware)}\" failed", exception);
            }
        }
    }
}