using System;
using Kolyhalov.FatNetLib.Core.Loggers;

namespace Kolyhalov.FatNetLib.UnityLogging
{
    public class UnityLogger : ILogger
    {
        private readonly LogLevel _minimumLogLevel;

        public UnityLogger(LogLevel minimumLogLevel)
        {
            _minimumLogLevel = minimumLogLevel;
        }

        public void Debug(string message)
        {
            if (_minimumLogLevel > LogLevel.Debug)
                return;

            UnityEngine.Debug.Log(message);
        }

        public void Debug(Func<string> messageProvider)
        {
            if (_minimumLogLevel > LogLevel.Debug)
                return;

            UnityEngine.Debug.Log(messageProvider.Invoke());
        }

        public void Info(string message)
        {
            if (_minimumLogLevel > LogLevel.Info)
                return;

            UnityEngine.Debug.Log(message);
        }

        public void Warn(string message)
        {
            if (_minimumLogLevel > LogLevel.Warn)
                return;

            UnityEngine.Debug.LogWarning(message);
        }

        public void Error(Exception exception, string message)
        {
            if (_minimumLogLevel > LogLevel.Error)
                return;

            exception.Data["LogMessage"] = message;
            UnityEngine.Debug.LogException(exception);
        }

        public void Error(string message)
        {
            if (_minimumLogLevel > LogLevel.Error)
                return;

            UnityEngine.Debug.LogError(message);
        }
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Warn,
        Error
    }
}
