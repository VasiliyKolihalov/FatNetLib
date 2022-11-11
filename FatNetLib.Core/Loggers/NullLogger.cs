using System;

namespace Kolyhalov.FatNetLib.Core.Loggers
{
    public class NullLogger : ILogger
    {
        public void Debug(string message)
        {
            // no action required
        }

        public void Debug(Func<string> messageProvider)
        {
            // no action required
        }

        public void Info(string message)
        {
            // no action required
        }

        public void Warn(string message)
        {
            // no action required
        }

        public void Error(Exception exception, string message)
        {
            // no action required
        }

        public void Error(string message)
        {
            // no action required
        }
    }
}
