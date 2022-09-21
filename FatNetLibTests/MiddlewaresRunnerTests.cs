using System;
using System.Collections.Generic;
using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.FatNetLib;
using Kolyhalov.FatNetLib.Middlewares;
using Moq;
using NUnit.Framework;
using static Moq.Times;

namespace FatNetLibTests;

public class MiddlewaresRunnerTests
{
    [Test, AutoData]
    public void Process_PassingMiddlewares_AllMiddlewaresCalled(Package inputPackage)
    {
        // Arrange
        Mock<IMiddleware> middleware1 = APassingMiddleware();
        Mock<IMiddleware> middleware2 = APassingMiddleware();
        var middlewares = new List<IMiddleware> { middleware1.Object, middleware2.Object };
        var middlewaresRunner = new MiddlewaresRunner(middlewares);

        // Act
        var outputPackage = middlewaresRunner.Process(inputPackage);

        // Assert
        middleware1.Verify(_ => _.Process(inputPackage), Once);
        middleware2.Verify(_ => _.Process(inputPackage), Once);
        outputPackage.Should().Be(inputPackage);
    }

    [Test, AutoData]
    public void Process_ReplacingMiddlewares_PackageIsReplaced(Package inputPackage, Package replacedPackage)
    {
        // Arrange
        Mock<IMiddleware> middleware1 = AReplacingMiddleware(replacedPackage);
        var middlewares = new List<IMiddleware> { middleware1.Object };
        var middlewaresRunner = new MiddlewaresRunner(middlewares);

        // Act
        var outputPackage = middlewaresRunner.Process(inputPackage);

        // Assert
        middleware1.Verify(_ => _.Process(inputPackage), Once);
        outputPackage.Should().Be(replacedPackage);
    }

    [Test, AutoData]
    public void Process_Middlewares_MiddlewaresCalledInOrder(Package inputPackage, 
        Package middleware1Output,
        Package middleware2Output)
    {
        // Arrange
        Mock<IMiddleware> middleware1 = AReplacingMiddleware(middleware1Output);
        Mock<IMiddleware> middleware2 = AReplacingMiddleware(middleware2Output);
        var middlewares = new List<IMiddleware> { middleware1.Object, middleware2.Object };
        var middlewaresRunner = new MiddlewaresRunner(middlewares);

        // Act
        var outputPackage = middlewaresRunner.Process(inputPackage);

        // Assert
        middleware1.Verify(_ => _.Process(inputPackage), Once);
        middleware2.Verify(_ => _.Process(middleware1Output), Once);
        outputPackage.Should().Be(middleware2Output);
    }
    
    [Test, AutoData]
    public void Process_ThrowingMiddlewares_Throw(Package inputPackage)
    {
        // Arrange
        var middleware = new Mock<IMiddleware>();
        middleware.Setup(_ => _.Process(It.IsAny<Package>()))
            .Throws(new ArithmeticException());
        var middlewares = new List<IMiddleware> { middleware.Object };
        var middlewaresRunner = new MiddlewaresRunner(middlewares);

        // Act
        Action act = () => middlewaresRunner.Process(inputPackage);

        // Assert
        act.Should().Throw<FatNetLibException>()
            .WithMessage("Middleware failed while processing package")
            .WithInnerException<ArithmeticException>();
    }

    private static Mock<IMiddleware> APassingMiddleware()
    {
        var middlewareRunner = new Mock<IMiddleware>();
        middlewareRunner.Setup(_ => _.Process(It.IsAny<Package>()))
            .Returns<Package>(package => package);
        return middlewareRunner;
    }

    private static Mock<IMiddleware> AReplacingMiddleware(Package replacedPackage)
    {
        var middlewareRunner = new Mock<IMiddleware>();
        middlewareRunner.Setup(_ => _.Process(It.IsAny<Package>()))
            .Returns(replacedPackage);
        return middlewareRunner;
    }
}