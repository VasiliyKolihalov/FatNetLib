using Kolyhalov.FatNetLib.Core.Modules;
using Microsoft.Extensions.Logging;
using ILogger = Kolyhalov.FatNetLib.Core.Loggers.ILogger;
using IMicrosoftLogger = Microsoft.Extensions.Logging.ILogger;

namespace Kolyhalov.FatNetLib.MicrosoftLogging
{
    public class MicrosoftLoggerModule : IModule
    {
        private readonly LogLevel _minimumLogLevel;

        public MicrosoftLoggerModule(LogLevel minimumLogLevel = LogLevel.Information)
        {
            _minimumLogLevel = minimumLogLevel;
        }

        public void Setup(IModuleContext moduleContext)
        {
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
}
