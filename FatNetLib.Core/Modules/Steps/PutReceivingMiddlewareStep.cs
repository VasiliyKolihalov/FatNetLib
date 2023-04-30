using System;
using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Middlewares;
using Kolyhalov.FatNetLib.Core.Storages;

namespace Kolyhalov.FatNetLib.Core.Modules.Steps
{
    public class PutReceivingMiddlewareStep<T> : IStep where T : IMiddleware
    {
        private readonly Func<IDependencyContext, T> _middlewareProvider;
        private readonly IDependencyContext _dependencyContext;

        public PutReceivingMiddlewareStep(
            Func<IDependencyContext, T> middlewareProvider,
            IDependencyContext dependencyContext)
        {
            _middlewareProvider = middlewareProvider;
            _dependencyContext = dependencyContext;
        }

        public object Qualifier => typeof(T);

        public void Run()
        {
            _dependencyContext.Get<IList<IMiddleware>>("ReceivingMiddlewares")
                .Add(_middlewareProvider.Invoke(_dependencyContext));
        }
    }
}
