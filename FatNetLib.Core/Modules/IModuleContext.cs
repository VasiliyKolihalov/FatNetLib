using System;
using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Controllers;
using Kolyhalov.FatNetLib.Core.Modules.Steps;
using Kolyhalov.FatNetLib.Core.Storages;

namespace Kolyhalov.FatNetLib.Core.Modules
{
    public interface IModuleContext
    {
        public IModuleContext PutDependency<T>(Func<IDependencyContext, T> dependencyProvider) where T : class;

        public IModuleContext PutDependency(string id, Func<IDependencyContext, object> dependencyProvider);

        public IModuleContext PutModule(IModule module);

        public IModuleContext PutModules(IEnumerable<IModule> modules);

        public IModuleContext PutController<T>(Func<IDependencyContext, T> controllerProvider) where T : IController;

        public IModuleContext PutScript(string name, Action<IDependencyContext> script);

        public FindModuleContext FindModule(ModuleId moduleId);

        public FindStepContext FindStep(StepId stepId);

        public FindStepContext TakeLastStep();
    }
}
