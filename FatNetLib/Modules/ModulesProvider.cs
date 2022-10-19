namespace Kolyhalov.FatNetLib.Modules;

public class ModulesProvider : IModulesProvider
{
    private readonly ModuleContext _moduleContext;

    public ModulesProvider(ModuleContext moduleContext)
    {
        _moduleContext = moduleContext;
    }

    public IModulesProvider Register(IModule module)
    {
        module.OnRegister(_moduleContext);
        return this;
    }
}