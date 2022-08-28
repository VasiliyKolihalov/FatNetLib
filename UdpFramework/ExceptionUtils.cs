using Microsoft.Extensions.Logging;

namespace Kolyhalov.UdpFramework;

public static class ExceptionUtils
{
    public static void CatchExceptionsTo(ILogger? logger, Action @try, string exceptionMsg = "Exception occurred")
    {
        try
        {
            @try.Invoke();
        }
        catch (Exception e)
        {
            logger?.LogError(e, exceptionMsg);
        }
    }
}