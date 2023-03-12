using System;
using System.Collections.Generic;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Middlewares;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Modules.Steps;
using Kolyhalov.FatNetLib.Core.Storages;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests.Modules.Steps;

public class SortMiddlewaresStepTests
{
    public const string ReceivingMiddlewaresId = "ReceivingMiddlewares";

    public const string SendingMiddlewaresId = "SendingMiddlewares";

    private readonly IDependencyContext _dependencyContext = new DependencyContext();

    public IList<IMiddleware> SendingMiddlewares =>
        _dependencyContext.Get<IList<IMiddleware>>(SendingMiddlewaresId);

    public IList<IMiddleware> ReceivingMiddlewares =>
        _dependencyContext.Get<IList<IMiddleware>>(ReceivingMiddlewaresId);

    [SetUp]
    public void SetUp()
    {
        _dependencyContext.Put(SendingMiddlewaresId, new List<IMiddleware>
        {
            new MiddlewareC(), new MiddlewareB(), new MiddlewareA()
        });

        _dependencyContext.Put(ReceivingMiddlewaresId, new List<IMiddleware>
        {
            new MiddlewareC(), new MiddlewareB(), new MiddlewareA()
        });
    }

    [Test]
    public void Run_SendingOrder_Sort()
    {
        // Arrange
        IList<IMiddleware> sendingMiddlewares = SendingMiddlewares;
        var order = new List<Type>
        {
            typeof(MiddlewareA),
            typeof(MiddlewareB),
            typeof(MiddlewareC)
        };
        var step = new SortMiddlewaresStep(order, _dependencyContext, SendingMiddlewaresId);

        // Act
        step.Run();

        // Assert
        SendingMiddlewares.Should().BeSameAs(sendingMiddlewares);
        SendingMiddlewares[0].Should().BeOfType<MiddlewareA>();
        SendingMiddlewares[1].Should().BeOfType<MiddlewareB>();
        SendingMiddlewares[2].Should().BeOfType<MiddlewareC>();
    }

    [Test]
    public void Run_MiddlewaresWithSameTypes_Sort()
    {
        // Arrange
        var middleware1 = new MiddlewareA();
        var middleware2 = new MiddlewareA();
        _dependencyContext.Put(SendingMiddlewaresId, new List<IMiddleware>
        {
            middleware1, middleware2
        });
        IList<IMiddleware> sendingMiddlewares = SendingMiddlewares;
        var order = new List<Type>
        {
            typeof(MiddlewareA),
            typeof(MiddlewareA)
        };
        var step = new SortMiddlewaresStep(order, _dependencyContext, SendingMiddlewaresId);

        // Act
        step.Run();

        // Assert
        SendingMiddlewares.Should().BeSameAs(sendingMiddlewares);
        SendingMiddlewares[0].Should().BeSameAs(middleware1);
        SendingMiddlewares[1].Should().BeSameAs(middleware2);
    }

    [Test]
    public void Run_MiddlewareNotExists_Throw()
    {
        // Arrange
        var order = new List<Type>
        {
            typeof(MiddlewareA),
            typeof(MiddlewareB),
            typeof(MiddlewareD)
        };
        var step = new SortMiddlewaresStep(order, _dependencyContext, SendingMiddlewaresId);

        // Act
        Action act = () => step.Run();

        // Assert
        act.Should().Throw<FatNetLibException>().WithMessage(
            "Failed to sort middlewares. Middleware with type " +
            "Kolyhalov.FatNetLib.Core.Tests.Modules.Steps.SortMiddlewaresStepTests+MiddlewareD not found");
    }

    [Test]
    public void Run_WrongNumberOfTypes_Throw()
    {
        // Arrange
        var order = new List<Type>
        {
            typeof(MiddlewareA),
            typeof(MiddlewareB)
        };
        var step = new SortMiddlewaresStep(order, _dependencyContext, SendingMiddlewaresId);

        // Act
        Action act = () => step.Run();

        // Assert
        act.Should().Throw<FatNetLibException>().WithMessage(
            "Failed to sort middlewares. Count of types does not match the count of middlewares");
    }

    private class MiddlewareA : IMiddleware
    {
        public void Process(Package package)
        {
            throw new NotImplementedException();
        }
    }

    private class MiddlewareB : IMiddleware
    {
        public void Process(Package package)
        {
            throw new InvalidOperationException("This test method shouldn't be called");
        }
    }

    private class MiddlewareC : IMiddleware
    {
        public void Process(Package package)
        {
            throw new InvalidOperationException("This test method shouldn't be called");
        }
    }

    private class MiddlewareD : IMiddleware
    {
        public void Process(Package package)
        {
            throw new InvalidOperationException("This test method shouldn't be called");
        }
    }
}
