using System;
using System.Threading.Tasks;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Models;

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

        public static async Task CatchExceptionsToAsync(
            ILogger logger,
            Func<Task> @try,
            string message = "Exception occurred")
        {
            try
            {
                await @try.Invoke();
            }
            catch (Exception exception)
            {
                logger.Error(exception, message);
            }
        }

        public static EndpointRunFailedView ToEndpointRunFailedView(this Exception exception) =>
            new EndpointRunFailedView
            {
                Message = exception.Message,
                Type = exception.GetType(),
                InnerExceptionView = exception.InnerException?.ToEndpointRunFailedView() ?? null
            };
    }
}
