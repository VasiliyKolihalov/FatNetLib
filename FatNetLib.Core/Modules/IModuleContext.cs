using System;
using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Controllers;
using Kolyhalov.FatNetLib.Core.Middlewares;
using Kolyhalov.FatNetLib.Core.Modules.Steps;
using Kolyhalov.FatNetLib.Core.Storages;

namespace Kolyhalov.FatNetLib.Core.Modules
{
    public interface IModuleContext
    {
        public IModuleContext PutDependency(string id, Func<IDependencyContext, object> dependencyProvider);

        public IModuleContext PutDependency<T>(Func<IDependencyContext, T> dependencyProvider) where T : class;

        public IModuleContext PutModule(IModule module);

        public IModuleContext PutModules(IEnumerable<IModule> modules);

        public IModuleContext PutController<T>(Func<IDependencyContext, T> controllerProvider) where T : IController;

        public IModuleContext PutScript(string name, Action<IDependencyContext> script);

        public IModuleContext PatchConfiguration(Configuration configurationPatch);

        public IModuleContext PutSendingMiddleware<T>(Func<IDependencyContext, T> middlewareProvider)
            where T : IMiddleware;

        public IModuleContext PutReceivingMiddleware<T>(Func<IDependencyContext, T> middlewareProvider)
            where T : IMiddleware;

        public IModuleContext SortSendingMiddlewares(IEnumerable<Type> middlewaresOrder);

        public IModuleContext SortReceivingMiddlewares(IEnumerable<Type> middlewaresOrder);

        public IFindModuleContext FindModule(ModuleId moduleId);

        public IFindStepContext FindStep(StepId stepId);

        public IFindStepContext FindStep(ModuleId parent, Type step, object qualifier);

        public IFindStepContext FindStep(ModuleId parent, StepType step, object qualifier);

        // Todo: create TakeNextStep()

        public interface IFindModuleContext
        {
            public IModuleContext AndRemoveIt();
        }

        public interface IFindStepContext
        {
            public IModuleContext AndRemoveIt();

            public IModuleContext AndMoveBeforeStep(ModuleId parent, Type step, object qualifier);

            public IModuleContext AndMoveBeforeStep(ModuleId parent, StepType step, object qualifier);

            public IModuleContext AndMoveAfterStep(ModuleId parent, Type step, object qualifier);

            public IModuleContext AndMoveAfterStep(ModuleId parent, StepType step, object qualifier);

            public IModuleContext AndReplaceOld(ModuleId parent, Type step, Type qualifier);

            public IModuleContext AndReplaceOld(ModuleId parent, StepType step, Type qualifier);
        }
    }
}
