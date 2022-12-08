using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Modules;

namespace Kolyhalov.FatNetLib.UnityLogging
{
    public class UnityLoggerModule : IModule
    {
        private readonly LogLevel _minimalLogLevel;

        public UnityLoggerModule(LogLevel minimalLogLevel = LogLevel.Info)
        {
            _minimalLogLevel = minimalLogLevel;
        }

        public void Setup(IModuleContext moduleContext)
        {
            moduleContext.PutDependency<ILogger>(_ => new UnityLogger(_minimalLogLevel));
        }
    }
}
