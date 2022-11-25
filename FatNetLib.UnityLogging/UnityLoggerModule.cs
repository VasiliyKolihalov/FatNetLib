using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Modules;

namespace Kolyhalov.FatNetLib.UnityLogging
{
    public class UnityLoggerModule : IModule
    {
        public void Setup(IModuleContext moduleContext)
        {
            moduleContext.PutDependency<ILogger>(_ => new UnityLogger());
        }
    }
}
