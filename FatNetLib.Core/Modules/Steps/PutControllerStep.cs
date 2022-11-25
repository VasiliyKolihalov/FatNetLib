using System;
using Kolyhalov.FatNetLib.Core.Controllers;
using Kolyhalov.FatNetLib.Core.Recorders;
using Kolyhalov.FatNetLib.Core.Storages;

namespace Kolyhalov.FatNetLib.Core.Modules.Steps
{
    public class PutControllerStep<T> : IModuleStep where T : IController
    {
        private readonly Func<IDependencyContext, T> _controllerProvider;
        private readonly IDependencyContext _dependencyContext;

        public PutControllerStep(
            Func<IDependencyContext, T> controllerProvider,
            Type parentModuleType,
            IDependencyContext dependencyContext)
        {
            _controllerProvider = controllerProvider;
            Id = new StepId(parentModuleType, GetType(), typeof(T));
            _dependencyContext = dependencyContext;
        }

        public StepId Id { get; }

        public void Run()
        {
            T controller = _controllerProvider.Invoke(_dependencyContext);
            _dependencyContext.Get<IEndpointRecorder>().AddController(controller);
        }

        public IModuleStep CopyWithNewId(StepId newId)
        {
            return new PutControllerStep<T>(_controllerProvider, newId.ParentModuleType, _dependencyContext);
        }
    }
}
