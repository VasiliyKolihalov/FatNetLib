﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Monitors;
using Kolyhalov.FatNetLib.Core.Runners;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Wrappers;
using Moq;
using NUnit.Framework;
using static System.Text.Encoding;

namespace Kolyhalov.FatNetLib.Core.Tests
{
    public class ServerCourierTests
    {
        private List<INetPeer> _connectedPeers = null!;
        private IEndpointsStorage _endpointsStorage = null!;
        private ServerCourier _courier = null!;

        [SetUp]
        public void SetUp()
        {
            _connectedPeers = new List<INetPeer>();
            _endpointsStorage = new EndpointsStorage();

            _courier = new ServerCourier(
                _connectedPeers,
                _endpointsStorage,
                new Mock<IResponsePackageMonitor>().Object,
                AMiddlewareRunner().Object,
                new Mock<IEndpointsInvoker>().Object,
                new Mock<ILogger>().Object);
        }

        [Test]
        public void Broadcast_CorrectCase_Pass()
        {
            // Arrange
            var route = new Route("correct-route");
            List<Mock<INetPeer>> peers = ANetPeers();
            _connectedPeers.AddRange(peers.Select(_ => _.Object));
            RegisterRemoteEndpoints(peers.Select(_ => _.Object), route);
            var package = new Package { Route = route };

            // Act
            _courier.Broadcast(package);

            // Assert
            foreach (Mock<INetPeer> peer in peers)
            {
                peer.Verify(_ => _.Send(package));
            }
        }

        [Test]
        public void Broadcast_IgnorePeer_Pass()
        {
            // Arrange
            var route = new Route("correct-route");
            List<Mock<INetPeer>> peers = ANetPeers();
            _connectedPeers.AddRange(peers.Select(_ => _.Object));
            RegisterRemoteEndpoints(peers.Select(_ => _.Object), route);
            var package = new Package { Route = route };
            var peerIdToIgnore = 0;

            // Act
            _courier.Broadcast(package, peerIdToIgnore);

            // Assert
            foreach (Mock<INetPeer> peer in peers)
            {
                if (peer.Object.Id == peerIdToIgnore)
                {
                    peer.Verify(_ => _.Send(package), Times.Never);
                    continue;
                }

                peer.Verify(_ => _.Send(package));
            }
        }

        private static List<Mock<INetPeer>> ANetPeers()
        {
            var peer1 = new Mock<INetPeer>();
            peer1.Setup(_ => _.Id).Returns(0);
            var peer2 = new Mock<INetPeer>();
            peer2.Setup(_ => _.Id).Returns(1);

            return new List<Mock<INetPeer>> { peer1, peer2 };
        }

        private void RegisterRemoteEndpoints(IEnumerable<INetPeer> peers, Route route)
        {
            foreach (INetPeer peer in peers)
            {
                _endpointsStorage.RemoteEndpoints[peer.Id] = new List<Endpoint>
                {
                    new Endpoint(
                        route,
                        EndpointType.Receiver,
                        Reliability.Sequenced,
                        new PackageSchema(),
                        new PackageSchema())
                };
            }
        }

        private static Mock<IMiddlewaresRunner> AMiddlewareRunner()
        {
            var middlewareRunner = new Mock<IMiddlewaresRunner>();
            middlewareRunner.Setup(_ => _.Process(It.IsAny<Package>()))
                .Callback<Package>(package => { package.Serialized = UTF8.GetBytes("serialized-package"); });
            return middlewareRunner;
        }
    }
}
