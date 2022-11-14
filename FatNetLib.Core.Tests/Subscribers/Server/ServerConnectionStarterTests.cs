﻿using Kolyhalov.FatNetLib.Core.Microtypes;
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
            _starter = new ServerConnectionStarter(_netManager.Object, new Port(123));
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
    }
}