namespace Kolyhalov.FatNetLib.Modules;

public abstract class Module
{
    public abstract void Setup(ModuleContext moduleContext);
    public IList<Module> ChildModules { get; } = new List<Module>();
}