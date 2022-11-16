using System;
using System.Collections.Generic;
using System.Linq;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Monitors;
using Kolyhalov.FatNetLib.Core.Runners;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core
{
    public class Courier : ICourier
    {
        protected readonly IList<INetPeer> ConnectedPeers;
        private readonly IEndpointsStorage _endpointsStorage;
        private readonly IResponsePackageMonitor _responsePackageMonitor;
        private readonly IMiddlewaresRunner _sendingMiddlewaresRunner;

        public Courier(
            IList<INetPeer> connectedPeers,
            IEndpointsStorage endpointsStorage,
            IResponsePackageMonitor responsePackageMonitor,
            IMiddlewaresRunner sendingMiddlewaresRunner)
        {
            ConnectedPeers = connectedPeers;
            _endpointsStorage = endpointsStorage;
            _responsePackageMonitor = responsePackageMonitor;
            _sendingMiddlewaresRunner = sendingMiddlewaresRunner;
        }

        public Package? SendPackage(Package package)
        {
            if (package is null) throw new ArgumentNullException(nameof(package));

            if (package.Route is null) throw new ArgumentNullException(nameof(package.Route));

            int toPeerId = package.ToPeerId
                           ?? throw new ArgumentNullException(nameof(package.ToPeerId));

            INetPeer peer = ConnectedPeers.FirstOrDefault(peer => peer.Id == toPeerId) ??
                            throw new FatNetLibException("Receiving peer not found");

            Endpoint endpoint = _endpointsStorage.RemoteEndpoints[toPeerId]
                                    .FirstOrDefault(endpoint => endpoint.Route.Equals(package.Route)) ??
                                throw new FatNetLibException("Endpoint not found");

            package.Reliability = endpoint.Reliability;
            if (endpoint.EndpointType is EndpointType.Exchanger && package.ExchangeId == Guid.Empty)
            {
                package.ExchangeId = Guid.NewGuid();
            }

            _sendingMiddlewaresRunner.Process(package);
            if (package.Serialized is null)
                throw new FatNetLibException($"{nameof(package.Serialized)} field is missing");
            peer.Send(package);

            return endpoint.EndpointType switch
            {
                EndpointType.Receiver => null,
                EndpointType.Exchanger => _responsePackageMonitor.Wait(package.ExchangeId),
                _ => throw new FatNetLibException($"Unsupported {nameof(EndpointType)} {endpoint.EndpointType}")
            };
        }
    }
}
