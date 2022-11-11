using System;
using Kolyhalov.FatNetLib.Core.Loggers;

namespace Kolyhalov.FatNetLib.Core.Utils
{
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
                logger.Error(exception, message);
            }
        }
    }
}
