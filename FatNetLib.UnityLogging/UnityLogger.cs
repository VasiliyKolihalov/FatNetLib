using System;
using Kolyhalov.FatNetLib.Core.Loggers;

namespace Kolyhalov.FatNetLib.UnityLogging
{
    public class UnityLogger : ILogger
    {
        private readonly LogLevel _minimalLogLevel;

        public UnityLogger(LogLevel minimalLogLevel)
        {
            _minimalLogLevel = minimalLogLevel;
        }

        public void Debug(string message)
        {
            if (_minimalLogLevel > LogLevel.Info)
                return;

            UnityEngine.Debug.Log(message);
        }

        public void Debug(Func<string> messageProvider)
        {
            if (_minimalLogLevel > LogLevel.Info)
                return;

            UnityEngine.Debug.Log(messageProvider.Invoke());
        }

        public void Info(string message)
        {
            if (_minimalLogLevel > LogLevel.Info)
                return;

            UnityEngine.Debug.Log(message);
        }

        public void Warn(string message)
        {
            if (_minimalLogLevel > LogLevel.Warn)
                return;

            UnityEngine.Debug.LogWarning(message);
        }

        public void Error(Exception exception, string message)
        {
            if (_minimalLogLevel > LogLevel.Error)
                return;

            exception.Data["LogMessage"] = message;
            UnityEngine.Debug.LogException(exception);
        }

        public void Error(string message)
        {
            if (_minimalLogLevel > LogLevel.Error)
                return;

            UnityEngine.Debug.LogError(message);
        }
    }

    public enum LogLevel
    {
        Info,
        Warn,
        Error
    }
}
