using Microsoft.Extensions.Logging;

namespace Kolyhalov.FatNetLib;

public static class ExceptionUtils
{
    public static void CatchExceptionsTo(ILogger? logger, Action @try, string exceptionMessage = "Exception occurred")
    {
        try
        {
            @try.Invoke();
        }
        catch (Exception exception)
        {
            logger?.LogError(exception, exceptionMessage);
        }
    }
}