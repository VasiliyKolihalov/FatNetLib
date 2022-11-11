using Kolyhalov.FatNetLib.Core.Modules;
using Microsoft.Extensions.Logging;
using ILogger = Kolyhalov.FatNetLib.Core.Loggers.ILogger;

namespace Kolyhalov.FatNetLib.MicrosoftLogging;

public class LoggerModule : IModule
{
    public void Setup(ModuleContext moduleContext)
    {
        Microsoft.Extensions.Logging.ILogger logger = LoggerFactory
            .Create(builder => builder.AddConsole())
            .CreateLogger<Core.FatNetLib>();

        moduleContext.DependencyContext.Put<ILogger>(_ => new MicrosoftLogger(logger));
    }

    public IList<IModule>? ChildModules => null;
}
