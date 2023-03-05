using System;
using Kolyhalov.FatNetLib.Core.Controllers;
using Kolyhalov.FatNetLib.Core.Recorders;
using Kolyhalov.FatNetLib.Core.Storages;

namespace Kolyhalov.FatNetLib.Core.Modules.Steps
{
    public class PutControllerStep<T> : IStep where T : IController
    {
        private readonly Func<IDependencyContext, T> _controllerProvider;
        private readonly IDependencyContext _dependencyContext;

        public PutControllerStep(Func<IDependencyContext, T> controllerProvider, IDependencyContext dependencyContext)
        {
            _controllerProvider = controllerProvider;
            _dependencyContext = dependencyContext;
        }

        public object Qualifier => typeof(T);

        public void Run()
        {
            T controller = _controllerProvider.Invoke(_dependencyContext);
            _dependencyContext.Get<IEndpointRecorder>().AddController(controller);
        }
    }
}
