using System;
using System.Collections.Generic;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Middlewares;
using Moq;
using NUnit.Framework;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Core.Tests.Middlewares
{
    public class MiddlewaresRunnerTests
    {
        [Test]
        public void Process_Middlewares_AllMiddlewaresCalled()
        {
            // Arrange
            var inputPackage = new Package();
            var middleware1 = new Mock<IMiddleware>();
            var middleware2 = new Mock<IMiddleware>();
            var middlewares = new List<IMiddleware> { middleware1.Object, middleware2.Object };
            var middlewaresRunner = new MiddlewaresRunner(middlewares);

            // Act
            middlewaresRunner.Process(inputPackage);

            // Assert
            middleware1.Verify(_ => _.Process(inputPackage), Once);
            middleware2.Verify(_ => _.Process(inputPackage), Once);
        }

        [Test]
        public void Process_Middlewares_MiddlewaresCalledInOrder()
        {
            // Arrange
            var package = new Package();
            var middleware1 = new Mock<IMiddleware>(MockBehavior.Strict);
            var middleware2 = new Mock<IMiddleware>(MockBehavior.Strict);
            var middlewares = new List<IMiddleware> { middleware1.Object, middleware2.Object };
            var middlewaresRunner = new MiddlewaresRunner(middlewares);

            var sequence = new MockSequence();
            middleware1.InSequence(sequence).Setup(x => x.Process(package));
            middleware2.InSequence(sequence).Setup(x => x.Process(package));

            // Act
            middlewaresRunner.Process(package);

            // Assert
            middleware1.Verify(_ => _.Process(package), Once);
            middleware2.Verify(_ => _.Process(package), Once);
        }

        [Test]
        public void Process_ThrowingMiddlewares_Throw()
        {
            // Arrange
            var inputPackage = new Package();
            var middleware = new Mock<IMiddleware>();
            middleware.Setup(_ => _.Process(It.IsAny<Package>()))
                .Throws(new ArithmeticException());
            var middlewares = new List<IMiddleware> { middleware.Object };
            var middlewaresRunner = new MiddlewaresRunner(middlewares);

            // Act
            Action act = () => middlewaresRunner.Process(inputPackage);

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Middleware \"IMiddlewareProxy\" failed")
                .WithInnerException<ArithmeticException>();
        }
    }
}
