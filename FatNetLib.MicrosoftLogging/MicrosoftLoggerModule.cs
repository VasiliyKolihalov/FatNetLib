using Kolyhalov.FatNetLib.Core.Modules;
using Microsoft.Extensions.Logging;
using ILogger = Kolyhalov.FatNetLib.Core.Loggers.ILogger;
using IMicrosoftLogger = Microsoft.Extensions.Logging.ILogger;

namespace Kolyhalov.FatNetLib.MicrosoftLogging
{
    public class MicrosoftLoggerModule : IModule
    {
        private readonly LogLevel _minimalLogLevel;

        public MicrosoftLoggerModule(LogLevel minimalLogLevel = LogLevel.Information)
        {
            _minimalLogLevel = minimalLogLevel;
        }

        public void Setup(IModuleContext moduleContext)
        {
            moduleContext
                .PutDependency<IMicrosoftLogger>(_ => LoggerFactory
                    .Create(builder =>
                    {
                        builder.AddConsole();
                        builder.SetMinimumLevel(_minimalLogLevel);
                    })
                    .CreateLogger<Core.FatNetLib>())
                .PutDependency<ILogger>(_ => new MicrosoftLogger(_.Get<IMicrosoftLogger>()));
        }
    }
}
