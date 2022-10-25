namespace Kolyhalov.FatNetLib.Modules;

public interface IModulesProvider
{
    public IModulesProvider Register(IModule module);
    public IModulesProvider Register(IList<IModule> modules);
}