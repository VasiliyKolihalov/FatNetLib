using System;
using System.Collections.Generic;
using System.Linq;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Middlewares;
using Kolyhalov.FatNetLib.Core.Storages;

namespace Kolyhalov.FatNetLib.Core.Modules.Steps
{
    public class SortMiddlewaresStep : IStep
    {
        private readonly IEnumerable<Type> _middlewareOrder;
        private readonly MiddlewaresType _middlewaresType;
        private readonly IDependencyContext _dependencyContext;

        public SortMiddlewaresStep(
            IEnumerable<Type> middlewareOrder,
            MiddlewaresType middlewaresType,
            IDependencyContext dependencyContext)
        {
            _middlewareOrder = middlewareOrder;
            _middlewaresType = middlewaresType;
            _dependencyContext = dependencyContext;
        }

        public object Qualifier => _middlewaresType;

        public void Run()
        {
            string middlewareDependencyId = _middlewaresType switch
            {
                MiddlewaresType.Sending => "SendingMiddlewares",
                MiddlewaresType.Receiving => "ReceivingMiddlewares",
                _ => throw new ArgumentOutOfRangeException(
                    nameof(_middlewaresType),
                    _middlewaresType,
                    "Unknown MiddlewaresType")
            };

            var middlewares = _dependencyContext.Get<IList<IMiddleware>>(middlewareDependencyId);
            IEnumerable<IMiddleware> sortedMiddleware = SortMiddlewaresByTypes(middlewares.ToList(), _middlewareOrder);
            middlewares.Clear();

            foreach (IMiddleware middleware in sortedMiddleware)
            {
                middlewares.Add(middleware);
            }
        }

        private static IEnumerable<IMiddleware> SortMiddlewaresByTypes(
            IList<IMiddleware> middlewares,
            IEnumerable<Type> middlewareTypes)
        {
            IList<IMiddleware> result = new List<IMiddleware>();

            foreach (Type middlewareType in middlewareTypes)
            {
                IMiddleware? middleware = middlewares.FirstOrDefault(_ => _.GetType() == middlewareType);
                if (middleware == null)
                    throw new FatNetLibException("Failed to sort middlewares. " +
                                                 $"Middleware with type {middlewareType} not found");
                result.Add(middleware);
            }

            if (result.Count != middlewares.Count)
                throw new FatNetLibException(
                    "Failed to sort middlewares. Number of types does not match the number of middlewares");

            return result;
        }
    }

    public enum MiddlewaresType
    {
        Sending,
        Receiving
    }
}
