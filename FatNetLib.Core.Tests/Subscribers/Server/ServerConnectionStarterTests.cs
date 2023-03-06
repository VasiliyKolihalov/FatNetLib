using System;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Subscribers.Server;
using Kolyhalov.FatNetLib.Core.Wrappers;
using Moq;
using NUnit.Framework;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Core.Tests.Subscribers.Server
{
    public class ServerConnectionStarterTests
    {
        private ServerConnectionStarter _starter = null!;
        private Mock<INetManager> _netManager = null!;

        [SetUp]
        public void SetUp()
        {
            _netManager = new Mock<INetManager>();
            _netManager.Setup(_ => _.Start(It.IsAny<int>()))
                .Returns(true);

            _starter = new ServerConnectionStarter(_netManager.Object, new ServerConfiguration
            {
                Port = new Port(123)
            });
        }

        [Test]
        public void StartConnection_CorrectCase_CallNetManager()
        {
            // Act
            _starter.StartConnection();

            // Assert
            _netManager.Verify(_ => _.Start(123), Once);
            _netManager.VerifyNoOtherCalls();
        }

        [Test]
        public void StartConnection_LiteNetLibNotStarted_Throws()
        {
            // Arrange
            _netManager.Setup(_ => _.Start(It.IsAny<int>()))
                .Returns(false);

            // Act
            Action act = () => _starter.StartConnection();

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Can't start server");
        }
    }
}
