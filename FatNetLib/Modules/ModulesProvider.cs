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
        module.Setup(_moduleContext);
        return this;
    }

    public IModulesProvider Register(IList<IModule> modules)
    {
        if (modules == null) throw new ArgumentNullException(nameof(modules));
        foreach (IModule module in modules)
        {
            module.Setup(_moduleContext);
        }
        return this;
    }
}