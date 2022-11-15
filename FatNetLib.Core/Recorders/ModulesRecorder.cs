using System;
using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Modules;

namespace Kolyhalov.FatNetLib.Core.Recorders
{
    public class ModulesRecorder : IModulesRecorder
    {
        private readonly IList<IModule> _modules = new List<IModule>();
        private readonly IList<Type> _ignoreModules = new List<Type>();
        private readonly IDictionary<Type, IModule> _moduleReplacement = new Dictionary<Type, IModule>();

        public IModulesRecorder Register(IModule module)
        {
            if (module == null) throw new ArgumentNullException(nameof(module));

            _modules.Add(module);
            return this;
        }

        public IModulesRecorder Register(IEnumerable<IModule> modules)
        {
            if (modules is null) throw new ArgumentNullException(nameof(modules));
            foreach (IModule module in modules)
            {
                Register(module);
            }

            return this;
        }

        public IModulesRecorder Ignore<T>() where T : IModule
        {
            _ignoreModules.Add(typeof(T));
            return this;
        }

        public IModulesRecorder Replace<T>(IModule module) where T : IModule
        {
            _moduleReplacement[typeof(T)] = module ?? throw new ArgumentNullException(nameof(module));
            return this;
        }

        public void Setup(ModuleContext moduleContext)
        {
            foreach (IModule module in _modules)
            {
                SetupModuleRecursive(moduleContext, module);
            }
        }

        private void SetupModuleRecursive(ModuleContext moduleContext, IModule module)
        {
            if (_ignoreModules.Contains(module.GetType()))
                return;

            if (_moduleReplacement.ContainsKey(module.GetType()))
            {
                module = _moduleReplacement[module.GetType()];
            }

            if (module.ChildModules != null)
            {
                foreach (IModule moduleChild in module.ChildModules)
                {
                    SetupModuleRecursive(moduleContext, moduleChild);
                }
            }

            module.Setup(moduleContext);
        }
    }
}
