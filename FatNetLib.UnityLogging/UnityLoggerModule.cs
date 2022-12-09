using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Modules;

namespace Kolyhalov.FatNetLib.UnityLogging
{
    public class UnityLoggerModule : IModule
    {
        private readonly LogLevel _minimumLogLevel;

        public UnityLoggerModule(LogLevel minimumLogLevel = LogLevel.Info)
        {
            _minimumLogLevel = minimumLogLevel;
        }

        public void Setup(IModuleContext moduleContext)
        {
            moduleContext.PutDependency<ILogger>(_ => new UnityLogger(_minimumLogLevel));
        }
    }
}
