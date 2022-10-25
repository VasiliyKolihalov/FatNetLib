using Kolyhalov.FatNetLib.Initializers.Controllers.Server;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules;

public class ServerInitialEndpointsModule : IModule
{
    public void Setup(ModuleContext moduleContext)
    {
        var exchangeEndpointsController = new ExchangeEndpointsController(moduleContext.EndpointsStorage);
        var initializationController = new InitializationController(moduleContext.EndpointsStorage);

        moduleContext.EndpointRecorder.AddController(exchangeEndpointsController);
        moduleContext.EndpointRecorder.AddController(initializationController);
    }
}