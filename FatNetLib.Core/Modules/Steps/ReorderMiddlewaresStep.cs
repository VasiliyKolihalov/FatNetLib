using System;
using System.Collections.Generic;
using System.Linq;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Middlewares;
using Kolyhalov.FatNetLib.Core.Storages;

namespace Kolyhalov.FatNetLib.Core.Modules.Steps
{
    public class ReorderMiddlewaresStep : IStep
    {
        private readonly IEnumerable<Type> _middlewareOrder;
        private readonly IDependencyContext _dependencyContext;
        private readonly string _dependencyId;

        public ReorderMiddlewaresStep(
            IEnumerable<Type> middlewareOrder,
            IDependencyContext dependencyContext,
            string dependencyId)
        {
            _middlewareOrder = middlewareOrder;
            _dependencyContext = dependencyContext;
            _dependencyId = dependencyId;
        }

        public object Qualifier => _dependencyId;

        public void Run()
        {
            var middlewares = _dependencyContext.Get<IList<IMiddleware>>(_dependencyId);

            if (_middlewareOrder.Count() != middlewares.Count)
                throw new FatNetLibException(
                    "Failed to reorder middlewares. Count of types does not match the count of middlewares");

            IList<IMiddleware> reorderMiddlewares = new List<IMiddleware>();

            foreach (Type middlewareType in _middlewareOrder)
            {
                IMiddleware? middleware = middlewares.FirstOrDefault(_ => _.GetType() == middlewareType);
                if (middleware == null)
                    throw new FatNetLibException("Failed to reorder middlewares. " +
                                                 $"Middleware with type {middlewareType} not found");
                middlewares.Remove(middleware);
                reorderMiddlewares.Add(middleware);
            }

            foreach (IMiddleware middleware in reorderMiddlewares)
            {
                middlewares.Add(middleware);
            }
        }
    }
}
