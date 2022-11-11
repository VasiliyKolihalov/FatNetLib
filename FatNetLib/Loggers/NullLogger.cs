using System;

namespace Kolyhalov.FatNetLib.Loggers
{
    public class NullLogger : ILogger
    {
        public void Debug(string message, params object?[] args)
        {
            // no actions required
        }

        public void Debug(Func<string> messageProvider, params object?[] args)
        {
            // no actions required
        }

        public void Info(string message, params object?[] args)
        {
            // no actions required
        }

        public void Warn(string message, params object?[] args)
        {
            // no actions required
        }

        public void Error(Exception? exception, string message, params object?[] args)
        {
            // no actions required
        }

        public void Error(string message, params object?[] args)
        {
            // no actions required
        }
    }
}
