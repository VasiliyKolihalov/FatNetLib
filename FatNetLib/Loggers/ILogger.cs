using System;

namespace Kolyhalov.FatNetLib.Loggers
{
    public interface ILogger
    {
        public void Debug(string message, params object?[] args);

        public void Debug(Func<string> messageProvider, params object?[] args);

        public void Info(string message, params object?[] args);

        public void Warn(string message, params object?[] args);

        public void Error(Exception? exception, string message, params object?[] args);

        public void Error(string message, params object?[] args);
    }
}
