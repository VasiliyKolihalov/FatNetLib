using System;
using ILogger = Kolyhalov.FatNetLib.Core.Loggers.ILogger;

namespace Kolyhalov.FatNetLib.UnityLogging
{
    public class UnityLogger : ILogger
    {
        public void Debug(string message, params object?[] args)
        {
            UnityEngine.Debug.LogFormat(message, args);
        }

        public void Debug(Func<string> messageProvider, params object?[] args)
        {
            UnityEngine.Debug.LogFormat(messageProvider.Invoke(), args);
        }

        public void Info(string message, params object?[] args)
        {
            UnityEngine.Debug.LogFormat(message, args);
        }

        public void Warn(string message, params object?[] args)
        {
            UnityEngine.Debug.LogWarningFormat(message, args);
        }

        public void Error(Exception? exception, string message, params object?[] args)
        {
            UnityEngine.Debug.LogException(exception);
        }

        public void Error(string message, params object?[] args)
        {
            UnityEngine.Debug.LogErrorFormat(message, args);
        }
    }
}
