using System;
using System.Collections.Generic;
using System.Linq;
using Kolyhalov.FatNetLib.Core.Controllers;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Modules.Steps;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Utils;

// Todo: make corrections revertible
namespace Kolyhalov.FatNetLib.Core.Modules
{
    public class ModuleContext : IModuleContext
    {
        private readonly IModule _currentModule;
        private readonly List<IModuleStep> _steps = new List<IModuleStep>();
        private readonly IDependencyContext _dependencyContext;
        private readonly Type _currentModuleType;

        public ModuleContext(
            IModule currentModule,
            IDependencyContext dependencyContext)
        {
            _currentModule = currentModule;
            _dependencyContext = dependencyContext;
            _currentModuleType = currentModule.GetType();
        }

        public IModuleContext PutDependency<T>(Func<IDependencyContext, T> dependencyProvider) where T : class
        {
            return PutDependency(typeof(T).ToDependencyId(), dependencyProvider);
        }

        public IModuleContext PutDependency(string id, Func<IDependencyContext, object> dependencyProvider)
        {
            _steps.Add(new PutDependencyStep(_currentModuleType, id, dependencyProvider, _dependencyContext));
            return this;
        }

        public IModuleContext PutModule(IModule module)
        {
            var childContext = new ModuleContext(module, _dependencyContext);
            module.Setup(childContext);
            _steps.Add(new BeginModuleStep(module.GetType(), _currentModuleType));
            _steps.AddRange(childContext._steps);
            _steps.Add(new EndModuleStep(module.GetType(), _currentModuleType));
            return this;
        }

        public IModuleContext PutModules(IEnumerable<IModule> modules)
        {
            foreach (IModule module in modules)
            {
                PutModule(module);
            }

            return this;
        }

        public IModuleContext PutController<T>(Func<IDependencyContext, T> controllerProvider) where T : IController
        {
            _steps.Add(new PutControllerStep<T>(controllerProvider, _currentModuleType, _dependencyContext));
            return this;
        }

        public IModuleContext PutScript(string name, Action<IDependencyContext> script)
        {
            _steps.Add(new PutScriptStep(name, script, _currentModuleType, _dependencyContext));
            return this;
        }

        public FindModuleModuleContext FindModule(Type moduleType)
        {
            return new FindModuleModuleContext(moduleType, _steps, context: this);
        }

        public FindStepModuleContext FindStep(StepId stepId)
        {
            return new FindStepModuleContext(stepId, _steps, context: this);
        }

        public FindStepModuleContext TakeLastStep()
        {
            if (_steps.Last() is BeginModuleStep || _steps.Last() is EndModuleStep)
                throw new FatNetLibException("Can't take module step as last");

            return new FindStepModuleContext(_steps.Last().Id, _steps, context: this);
        }

        public void Build()
        {
            _currentModule.Setup(this);
            Verify(_steps);
            foreach (IModuleStep step in _steps)
            {
                step.Run();
            }
        }

        private static void Verify(IEnumerable<IModuleStep> steps)
        {
            var ids = new HashSet<StepId>();
            foreach (IModuleStep step in steps)
            {
                if (ids.Contains(step.Id))
                    throw new FatNetLibException(@$"Step with id {step.Id} is already present");

                ids.Add(step.Id);
            }
        }
    }
}
