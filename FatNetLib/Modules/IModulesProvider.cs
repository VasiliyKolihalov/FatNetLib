namespace Kolyhalov.FatNetLib.Modules;

public interface IModulesProvider
{
    public IModulesProvider Register(Module module);
    public IModulesProvider Register(IList<Module> modules);
    public IModulesProvider Ignore<T>() where T : Module;
    public IModulesProvider Replace<T>(Module module) where T : Module;
    public void Setup(ModuleContext moduleContext);
}