using System.Threading.Tasks;
using Kolyhalov.FatNetLib.Core.Loggers;

namespace Kolyhalov.FatNetLib.Core.Exceptions
{
    public static class TaskExtensions
    {
        public static void ContinueWithLogException(
            this Task @this,
            ILogger logger,
            string logMessage = "Exception occurred")
        {
            @this.ContinueWith(
                continuationAction: task => logger.Error(task.Exception!, logMessage),
                continuationOptions: TaskContinuationOptions.OnlyOnFaulted);
        }
    }
}
