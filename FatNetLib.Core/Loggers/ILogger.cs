using System;

namespace Kolyhalov.FatNetLib.Core.Loggers
{
    public interface ILogger
    {
        public void Debug(string message);

        public void Debug(Func<string> messageProvider);

        public void Info(string message);

        public void Warn(string message);

        public void Error(Exception exception, string message);

        public void Error(string message);
    }
}
