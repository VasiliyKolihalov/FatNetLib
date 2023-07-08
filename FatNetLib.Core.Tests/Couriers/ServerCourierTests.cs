/*
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kolyhalov.FatNetLib.Core.Components;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Couriers;
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

namespace Kolyhalov.FatNetLib.Core.Tests.Couriers
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
        public async Task BroadcastAsync_CorrectCase_Pass()
        {
            // Arrange
            var route = new Route("correct-route");
            List<Mock<ISendingNetPeer>> peers = ANetPeers();
            _connectedPeers.AddRange(peers.Select(_ => _.Object));
            RegisterRemoteEndpoints(peers.Select(_ => _.Object), route);
            var package = new Package { Route = route };

            // Act
            await _courier.BroadcastAsync(package);

            // Assert
            foreach (Mock<ISendingNetPeer> peer in peers)
            {
                peer.Verify(_ => _.Send(package));
            }
        }

        [Test]
        public async Task BroadcastAsync_IgnorePeer_Pass()
        {
            // Arrange
            var route = new Route("correct-route");
            List<Mock<ISendingNetPeer>> peers = ANetPeers();
            _connectedPeers.AddRange(peers.Select(_ => _.Object));
            RegisterRemoteEndpoints(peers.Select(_ => _.Object), route);
            var package = new Package { Route = route };
            var peerIdToIgnore = 0;

            // Act
            await _courier.BroadcastAsync(package, peerIdToIgnore);

            // Assert
            foreach (Mock<ISendingNetPeer> peer in peers)
            {
                if (peer.Object.Id == peerIdToIgnore)
                {
                    peer.Verify(_ => _.Send(package), Times.Never);
                    continue;
                }

                peer.Verify(_ => _.Send(package));
            }
        }

        private static List<Mock<ISendingNetPeer>> ANetPeers()
        {
            var peer1 = new Mock<ISendingNetPeer>();
            peer1.Setup(_ => _.Id).Returns(0);
            var peer2 = new Mock<ISendingNetPeer>();
            peer2.Setup(_ => _.Id).Returns(1);

            return new List<Mock<ISendingNetPeer>> { peer1, peer2 };
        }

        private void RegisterRemoteEndpoints(IEnumerable<ISendingNetPeer> peers, Route route)
        {
            foreach (ISendingNetPeer peer in peers)
            {
                _endpointsStorage.RemoteEndpoints[peer.Id] = new List<Endpoint>
                {
                    new(
                        route,
                        EndpointType.Consumer,
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
*/
