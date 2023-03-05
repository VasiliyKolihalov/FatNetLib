using System;
using System.Threading.Tasks;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Providers;
using Kolyhalov.FatNetLib.Core.Subscribers.Client;
using Kolyhalov.FatNetLib.Core.Wrappers;
using Moq;
using NUnit.Framework;
using static LiteNetLib.ConnectionState;
using static Moq.Times;

namespace Kolyhalov.FatNetLib.Core.Tests.Subscribers.Client
{
    public class ClientConnectionStarterTests
    {
        private ClientConnectionStarter _starter = null!;
        private Mock<INetManager> _netManager = null!;
        private Mock<INetPeer> _serverPeer = null!;

        [SetUp]
        public void SetUp()
        {
            _serverPeer = new Mock<INetPeer>();
            _serverPeer.Setup(_ => _.ConnectionState)
                .Returns(Connected);

            _netManager = new Mock<INetManager>();
            _netManager.Setup(_ => _.Start())
                .Returns(true);
            _netManager.Setup(_ => _.Connect(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(_serverPeer.Object);

            var protocolVersionProvider = new Mock<IProtocolVersionProvider>();
            protocolVersionProvider.Setup(_ => _.Get())
                .Returns("test-protocol");
            var configuration = new ClientConfiguration
            {
                Address = "12.34.56.78",
                Port = new Port(123)
            };
            _starter = new ClientConnectionStarter(
                _netManager.Object,
                configuration,
                protocolVersionProvider.Object);
        }

        [Test]
        public void StartConnection_CorrectCase_CallNetManager()
        {
            // Act
            _starter.StartConnection();

            // Assert
            _netManager.Verify(_ => _.Start(), Once);
            _netManager.Verify(_ => _.Connect("12.34.56.78", 123, "test-protocol"), Once);
        }

        [Test]
        public void StartConnection_LiteNetLibNotStarted_Throws()
        {
            // Arrange
            _netManager.Setup(_ => _.Start())
                .Returns(false);

            // Act
            Action act = () => _starter.StartConnection();

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Can't start client");
        }

        [Test]
        public void StartConnection_LiteNetLibNotConnected_Throws()
        {
            // Arrange
            _serverPeer.Setup(_ => _.ConnectionState)
                .Returns(Outgoing);
            Task.Delay(20)
                .ContinueWith(_ =>
                    _serverPeer.Setup(__ => __.ConnectionState).Returns(Disconnected));

            // Act
            Action act = () => _starter.StartConnection();

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Can't connect client to the server");
        }
    }
}
