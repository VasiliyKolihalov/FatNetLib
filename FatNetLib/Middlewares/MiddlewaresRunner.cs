namespace Kolyhalov.FatNetLib;

public class MiddlewaresRunner : IMiddlewaresRunner
{
    private readonly IList<IMiddleware> _middlewares;

    public MiddlewaresRunner(IList<IMiddleware> middlewares)
    {
        _middlewares = middlewares;
    }

    public Package Process(Package package)
    {
        Package processingPackage = package;
        foreach (IMiddleware middleware in _middlewares)
        {
            try
            {
                processingPackage = middleware.Process(processingPackage);
            }
            catch (Exception exception)
            {
                throw new FatNetLibException("Middleware failed while processing package", exception);
            }
        }
        return processingPackage;
    }
}