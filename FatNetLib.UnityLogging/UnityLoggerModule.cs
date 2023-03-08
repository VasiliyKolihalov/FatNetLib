using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Modules;
using Kolyhalov.FatNetLib.Core.Modules.Defaults;
using Kolyhalov.FatNetLib.Core.Modules.Defaults.Client;
using static Kolyhalov.FatNetLib.Core.Modules.ModuleId.Pointers;
using static Kolyhalov.FatNetLib.Core.Modules.Steps.StepType;

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
            moduleContext
                .FindStep(
                    parent: ThisModule,
                    step: PutDependency,
                    qualifier: typeof(ILogger))
                .AndReplaceOld(
                    parent: ModuleId.Pointers.RootModule / typeof(DefaultClientModule) / typeof(DefaultCommonModule),
                    step: PutDependency,
                    qualifier: typeof(ILogger))
                .PutDependency<ILogger>(_ => new UnityLogger(_minimumLogLevel));
        }
    }
}
