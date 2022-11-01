namespace Kolyhalov.FatNetLib.Modules;

public interface IModulesRecorder
{
    public IModulesRecorder Register(IModule module);
    public IModulesRecorder Register(IList<IModule> modules);
    public IModulesRecorder Ignore<T>() where T : IModule;
    public IModulesRecorder Replace<T>(IModule module) where T : IModule;
    public void Setup(ModuleContext moduleContext);
}