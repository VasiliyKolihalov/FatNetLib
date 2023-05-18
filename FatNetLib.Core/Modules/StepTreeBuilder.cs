using System;
using System.Collections.Generic;
using System.Linq;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Controllers;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Middlewares;
using Kolyhalov.FatNetLib.Core.Modules.Steps;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Utils;
using static Kolyhalov.FatNetLib.Core.Modules.ModuleId.Pointers;

namespace Kolyhalov.FatNetLib.Core.Modules
{
    public class StepTreeBuilder : IModuleContext
    {
        private readonly IStepTreeNode _rootNode;
        private readonly IDependencyContext _dependencyContext;
        private IStepTreeNode _currentNode;

        public StepTreeBuilder(IModule rootModule, IDependencyContext dependencyContext)
        {
            _dependencyContext = dependencyContext;
            _rootNode = new StepTreeNode(new PutModuleStep(rootModule), parent: null);
            _currentNode = _rootNode;
        }

        public IStepTreeNode Build()
        {
            BuildModuleNodeRecursively();
            return _currentNode;
        }

        private void BuildModuleNodeRecursively()
        {
            var currentNodeModule = _currentNode.Step as PutModuleStep;
            currentNodeModule!.Module.Setup(moduleContext: this);

            foreach (IStepTreeNode childNode in _currentNode.ChildModuleNodes)
            {
                _currentNode = childNode;
                BuildModuleNodeRecursively();
                _currentNode = childNode.Parent!;
            }
        }

        public IModuleContext PutDependency(string id, Func<IDependencyContext, object> dependencyProvider)
        {
            AddStepToCurrentNode(new PutDependencyStep(id, dependencyProvider, _dependencyContext));
            return this;
        }

        public IModuleContext PutDependency<T>(Func<IDependencyContext, T> dependencyProvider) where T : class
        {
            return PutDependency(typeof(T).ToDependencyId(), dependencyProvider);
        }

        public IModuleContext PutModule(IModule module)
        {
            AddStepToCurrentNode(new PutModuleStep(module));
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
            AddStepToCurrentNode(new PutControllerStep<T>(controllerProvider, _dependencyContext));
            return this;
        }

        public IModuleContext PutScript(string name, Action<IDependencyContext> script)
        {
            AddStepToCurrentNode(new PutScriptStep(name, script, _dependencyContext));
            return this;
        }

        public IModuleContext PatchConfiguration(Configuration patch)
        {
            AddStepToCurrentNode(new PatchConfigurationStep(patch, _dependencyContext));
            return this;
        }

        public IModuleContext PatchDefaultPackageSchema(PackageSchema patch)
        {
            AddStepToCurrentNode(new PatchDefaultPackageSchemaStep(patch, _dependencyContext));
            return this;
        }

        public IModuleContext PutSendingMiddleware<T>(Func<IDependencyContext, T> middlewareProvider)
            where T : IMiddleware
        {
            AddStepToCurrentNode(new PutSendingMiddlewareStep<T>(middlewareProvider, _dependencyContext));
            return this;
        }

        public IModuleContext PutReceivingMiddleware<T>(Func<IDependencyContext, T> middlewareProvider)
            where T : IMiddleware
        {
            AddStepToCurrentNode(new PutReceivingMiddlewareStep<T>(middlewareProvider, _dependencyContext));
            return this;
        }

        public IModuleContext SortSendingMiddlewares(IEnumerable<Type> middlewaresOrder)
        {
            AddStepToCurrentNode(new SortMiddlewaresStep(
                middlewaresOrder,
                _dependencyContext,
                dependencyId: "SendingMiddlewares"));

            return this;
        }

        public IModuleContext SortReceivingMiddlewares(IEnumerable<Type> middlewaresOrder)
        {
            AddStepToCurrentNode(new SortMiddlewaresStep(
                middlewaresOrder,
                _dependencyContext,
                dependencyId: "ReceivingMiddlewares"));

            return this;
        }

        public IModuleContext.IFindModuleContext FindModule(ModuleId moduleId)
        {
            return new FindModuleContext(mainBuilder: this, CreateAbsoluteFromRelativeModuleId(moduleId));
        }

        public IModuleContext.IFindStepContext FindStep(StepId stepId)
        {
            return new FindStepContext(mainBuilder: this, CreateAbsoluteFromRelativeModuleId(stepId));
        }

        public IModuleContext.IFindStepContext FindStep(ModuleId parent, Type step, object qualifier)
        {
            return FindStep(new StepId(CreateAbsoluteFromRelativeModuleId(parent), step, qualifier));
        }

        public IModuleContext.IFindStepContext FindStep(ModuleId parent, StepType step, object qualifier)
        {
            return FindStep(parent, step.ToClass(), qualifier);
        }

        private void AddStepToCurrentNode(IStep step)
        {
            var newNode = new StepTreeNode(step, _currentNode);
            _currentNode.ChildNodes.Add(newNode);
        }

        private ModuleId CreateAbsoluteFromRelativeModuleId(ModuleId moduleId)
        {
            if (moduleId.Segments[0] == typeof(ThisModulePointer))
            {
                Type[] newSegments = _currentNode.BuildAbsolutePath()
                    .Concat(moduleId.Segments.Skip(1))
                    .ToArray();
                return new ModuleId(newSegments);
            }

            if (moduleId.Segments[0] == typeof(ParentModulePointer))
            {
                if (_currentNode.Parent == null)
                    throw new FatNetLibException("Current node doesn't have a parent");

                Type[] newSegments = _currentNode.Parent!.BuildAbsolutePath()
                    .Concat(moduleId.Segments.Skip(1))
                    .ToArray();
                return new ModuleId(newSegments);
            }

            if (moduleId.Segments[0] == typeof(RootModulePointer))
            {
                Type[] newSegments = Enumerable.Empty<Type>()
                    .Append((Type)_rootNode.Step.Qualifier)
                    .Concat(moduleId.Segments.Skip(1))
                    .ToArray();
                return new ModuleId(newSegments);
            }

            return moduleId;
        }

        private StepId CreateAbsoluteFromRelativeModuleId(StepId stepId)
        {
            ModuleId absoluteModuleId = CreateAbsoluteFromRelativeModuleId(stepId.ParentModuleId);
            return absoluteModuleId.Equals(stepId.ParentModuleId)
                ? stepId
                : new StepId(absoluteModuleId, stepId.StepType, stepId.Qualifier);
        }

        private sealed class FindModuleContext : IModuleContext.IFindModuleContext
        {
            private readonly StepTreeBuilder _mainBuilder;
            private readonly ModuleId _targetModuleId;

            public FindModuleContext(StepTreeBuilder mainBuilder, ModuleId targetModuleId)
            {
                _mainBuilder = mainBuilder;
                _targetModuleId = targetModuleId;
            }

            public IModuleContext AndRemoveIt()
            {
                _mainBuilder.AddStepToCurrentNode(new RemoveModuleStep(_mainBuilder._rootNode, _targetModuleId));
                return _mainBuilder;
            }
        }

        private sealed class FindStepContext : IModuleContext.IFindStepContext
        {
            private readonly StepTreeBuilder _mainBuilder;
            private readonly StepId _targetStepId;

            public FindStepContext(StepTreeBuilder mainBuilder, StepId targetStepId)
            {
                _mainBuilder = mainBuilder;
                _targetStepId = targetStepId;
            }

            public IModuleContext AndRemoveIt()
            {
                _mainBuilder.AddStepToCurrentNode(new RemoveStepStep(_mainBuilder._rootNode, _targetStepId));
                return _mainBuilder;
            }

            public IModuleContext AndMoveBeforeStep(ModuleId parent, Type step, object qualifier)
            {
                _mainBuilder.AddStepToCurrentNode(new MoveStepBeforeStep(
                    _mainBuilder._rootNode,
                    _targetStepId,
                    placeStepId: new StepId(_mainBuilder.CreateAbsoluteFromRelativeModuleId(parent), step, qualifier)));
                return _mainBuilder;
            }

            public IModuleContext AndMoveBeforeStep(ModuleId parent, StepType step, object qualifier)
            {
                return AndMoveBeforeStep(parent, step.ToClass(), qualifier);
            }

            public IModuleContext AndMoveAfterStep(ModuleId parent, Type step, object qualifier)
            {
                _mainBuilder.AddStepToCurrentNode(new MoveStepAfterStep(
                    _mainBuilder._rootNode,
                    _targetStepId,
                    placeStepId: new StepId(_mainBuilder.CreateAbsoluteFromRelativeModuleId(parent), step, qualifier)));
                return _mainBuilder;
            }

            public IModuleContext AndMoveAfterStep(ModuleId parent, StepType step, object qualifier)
            {
                return AndMoveAfterStep(parent, step.ToClass(), qualifier);
            }

            public IModuleContext AndReplaceOld(ModuleId parent, Type step, Type qualifier)
            {
                _mainBuilder.AddStepToCurrentNode(new ReplaceOldStepStep(
                    _mainBuilder._rootNode,
                    _targetStepId,
                    oldStepId: new StepId(_mainBuilder.CreateAbsoluteFromRelativeModuleId(parent), step, qualifier)));
                return _mainBuilder;
            }

            public IModuleContext AndReplaceOld(ModuleId parent, StepType step, Type qualifier)
            {
                return AndReplaceOld(parent, step.ToClass(), qualifier);
            }
        }
    }
}
