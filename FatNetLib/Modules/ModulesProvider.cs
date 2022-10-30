namespace Kolyhalov.FatNetLib.Modules;

public class ModulesProvider : IModulesProvider
{
    private readonly IList<Module> _modules = new List<Module>();
    private readonly IList<Type> _ignoreModules = new List<Type>();
    private readonly IDictionary<Type, Module> _moduleReplacement = new Dictionary<Type, Module>();

    public IModulesProvider Register(Module module)
    {
        _modules.Add(module);
        return this;
    }

    public IModulesProvider Register(IList<Module> modules)
    {
        if (modules == null) throw new ArgumentNullException(nameof(modules));
        foreach (Module module in modules)
        {
            Register(module);
        }
        return this;
    }

    public IModulesProvider Ignore<T>() where T : Module
    {
        _ignoreModules.Add(typeof(T));
        return this;
    }

    public IModulesProvider Replace<T>(Module module) where T : Module
    {
        _moduleReplacement[typeof(T)] = module;
        return this;
    }

    public void Setup(ModuleContext moduleContext)
    {
        foreach (Module module in _modules)
        {
            SetupModuleRecursive(moduleContext, module);
        }
    }

    private void SetupModuleRecursive(ModuleContext moduleContext, Module module)
    {
        if (_ignoreModules.Contains(module.GetType()))
            return;

        if (_moduleReplacement.ContainsKey(module.GetType()))
        {
            _moduleReplacement[module.GetType()].Setup(moduleContext);
            return;
        }
        module.Setup(moduleContext);
        
        if(module.ChildModules.Count < 0)
            return;
        
        foreach (Module moduleChild in module.ChildModules)
        {
            SetupModuleRecursive(moduleContext, moduleChild);
        }
    }
}