using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Modules;

namespace Kolyhalov.FatNetLib.UnityLogging
{
    public class LoggerModule : IModule
    {
        public void Setup(ModuleContext moduleContext)
        {
            moduleContext.DependencyContext.Put<ILogger>(_ => new UnityLogger());
        }

        public IList<IModule>? ChildModules => null;
    }
}
