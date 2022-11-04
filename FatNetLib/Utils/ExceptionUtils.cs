using Microsoft.Extensions.Logging;

namespace Kolyhalov.FatNetLib.Utils;

public static class ExceptionUtils
{
    public static void CatchExceptionsTo(ILogger logger, Action @try, string message = "Exception occurred")
    {
        try
        {
            @try.Invoke();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, message);
        }
    }
}
