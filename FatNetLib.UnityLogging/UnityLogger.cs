using System;
using Kolyhalov.FatNetLib.Core.Loggers;

namespace Kolyhalov.FatNetLib.UnityLogging
{
    public class UnityLogger : ILogger
    {
        public void Debug(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        public void Debug(Func<string> messageProvider)
        {
            UnityEngine.Debug.Log(messageProvider.Invoke());
        }

        public void Info(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        public void Warn(string message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        public void Error(Exception exception, string message)
        {
            exception.Data["LogMessage"] = message;
            UnityEngine.Debug.LogException(exception);
        }

        public void Error(string message)
        {
            UnityEngine.Debug.LogError(message);
        }
    }
}
