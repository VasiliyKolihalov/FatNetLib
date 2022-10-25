using Microsoft.Extensions.Logging;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules;

public class DefaultLoggerModule : IModule
{
    public void Setup(ModuleContext moduleContext)
    {
        moduleContext.LoggerProvider.Logger =
            LoggerFactory.Create(builder => { builder.AddConsole(); }).CreateLogger<FatNetLib>();
    }
}