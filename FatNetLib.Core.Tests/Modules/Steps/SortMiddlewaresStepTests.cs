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
    private IDependencyContext _dependencyContext = null!;

    public IList<IMiddleware> SendingMiddlewares => _dependencyContext.Get<IList<IMiddleware>>("SendingMiddlewares");

    public IList<IMiddleware> ReceivingMiddlewares =>
        _dependencyContext.Get<IList<IMiddleware>>("ReceivingMiddlewares");

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _dependencyContext = new DependencyContext();
    }

    [SetUp]
    public void SetUp()
    {
        _dependencyContext.Put("SendingMiddlewares", new List<IMiddleware>
        {
            new MiddlewareC(), new MiddlewareB(), new MiddlewareA()
        });

        _dependencyContext.Put("ReceivingMiddlewares", new List<IMiddleware>
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
        var step = new SortMiddlewaresStep(order, MiddlewaresType.Sending, _dependencyContext);

        // Act
        step.Run();

        // Assert
        SendingMiddlewares.Should().BeSameAs(sendingMiddlewares);
        SendingMiddlewares[0].Should().BeOfType<MiddlewareA>();
        SendingMiddlewares[1].Should().BeOfType<MiddlewareB>();
        SendingMiddlewares[2].Should().BeOfType<MiddlewareC>();
    }

    [Test]
    public void Run_ReceivingOrder_Sort()
    {
        // Arrange
        IList<IMiddleware> receivingMiddlewares = ReceivingMiddlewares;
        var order = new List<Type>
        {
            typeof(MiddlewareA),
            typeof(MiddlewareB),
            typeof(MiddlewareC)
        };
        var step = new SortMiddlewaresStep(order, MiddlewaresType.Receiving, _dependencyContext);

        // Act
        step.Run();

        // Assert
        ReceivingMiddlewares.Should().BeSameAs(receivingMiddlewares);
        ReceivingMiddlewares[0].Should().BeOfType<MiddlewareA>();
        ReceivingMiddlewares[1].Should().BeOfType<MiddlewareB>();
        ReceivingMiddlewares[2].Should().BeOfType<MiddlewareC>();
    }

    [Test]
    public void Run_NotExistMiddleware_Throw()
    {
        // Arrange
        var order = new List<Type>
        {
            typeof(MiddlewareA),
            typeof(MiddlewareB),
            typeof(MiddlewareC),
            typeof(MiddlewareD)
        };
        var step = new SortMiddlewaresStep(order, MiddlewaresType.Receiving, _dependencyContext);

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
        var step = new SortMiddlewaresStep(order, MiddlewaresType.Receiving, _dependencyContext);

        // Act
        Action act = () => step.Run();

        // Assert
        act.Should().Throw<FatNetLibException>().WithMessage(
            "Failed to sort middlewares. Number of types does not match the number of middlewares");
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
            throw new NotImplementedException();
        }
    }

    private class MiddlewareC : IMiddleware
    {
        public void Process(Package package)
        {
            throw new NotImplementedException();
        }
    }

    private class MiddlewareD : IMiddleware
    {
        public void Process(Package package)
        {
            throw new NotImplementedException();
        }
    }
}
