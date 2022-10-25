using Kolyhalov.FatNetLib.Initializers.Controllers.Client;

namespace Kolyhalov.FatNetLib.Modules.DefaultModules;

public class ClientInitialEndpointsModule : IModule
{
    public void Setup(ModuleContext moduleContext)
    {
        var controller = new ExchangeEndpointsController(moduleContext.EndpointsStorage);
        moduleContext.EndpointRecorder.AddController(controller);
    }
}