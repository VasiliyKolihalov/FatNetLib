using System;
using Kolyhalov.FatNetLib.Core.Modules;
using Kolyhalov.FatNetLib.Core.Modules.Defaults;
using Kolyhalov.FatNetLib.Core.Modules.Defaults.Client;
using Kolyhalov.FatNetLib.Core.Modules.Defaults.Server;
using Microsoft.Extensions.Logging;
using static Kolyhalov.FatNetLib.Core.Modules.ModuleId.Pointers;
using static Kolyhalov.FatNetLib.Core.Modules.Steps.StepType;
using static Kolyhalov.FatNetLib.MicrosoftLogging.For;
using ILogger = Kolyhalov.FatNetLib.Core.Loggers.ILogger;
using IMicrosoftLogger = Microsoft.Extensions.Logging.ILogger;

namespace Kolyhalov.FatNetLib.MicrosoftLogging
{
    public class MicrosoftLoggerModule : IModule
    {
        private readonly ModuleId _defaultCommonModuleId;
        private readonly LogLevel _minimumLogLevel;

        public MicrosoftLoggerModule(For @for, LogLevel minimumLogLevel = LogLevel.Information)
        {
            _defaultCommonModuleId = @for switch
            {
                Server => new ModuleId(
                    typeof(RootModulePointer), typeof(DefaultServerModule), typeof(DefaultCommonModule)),
                Client => new ModuleId(
                    typeof(RootModulePointer), typeof(DefaultClientModule), typeof(DefaultCommonModule)),
                _ => throw new ArgumentOutOfRangeException(nameof(@for), @for, null)
            };

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
                    parent: _defaultCommonModuleId,
                    step: PutDependency,
                    qualifier: typeof(ILogger));

            moduleContext
                .FindStep(
                    parent: ThisModule,
                    step: PutDependency,
                    qualifier: typeof(IMicrosoftLogger))
                .AndMoveBeforeStep(
                    parent: _defaultCommonModuleId,
                    step: PutDependency,
                    qualifier: typeof(ILogger));

            moduleContext
                .PutDependency<IMicrosoftLogger>(_ => LoggerFactory
                    .Create(builder =>
                    {
                        builder.AddConsole();
                        builder.SetMinimumLevel(_minimumLogLevel);
                    })
                    .CreateLogger<Core.FatNetLib>())
                .PutDependency<ILogger>(_ => new MicrosoftLogger(_.Get<IMicrosoftLogger>()));
        }
    }

    public enum For
    {
        Server,
        Client
    }
}
