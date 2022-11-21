﻿using System;
using System.Collections.Generic;
using System.Linq;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Loggers;
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
        private readonly IEndpointsInvoker _endpointsInvoker;
        private readonly ILogger _logger;

        public Courier(
            IList<INetPeer> connectedPeers,
            IEndpointsStorage endpointsStorage,
            IResponsePackageMonitor responsePackageMonitor,
            IMiddlewaresRunner sendingMiddlewaresRunner,
            IEndpointsInvoker endpointsInvoker,
            ILogger logger)
        {
            ConnectedPeers = connectedPeers;
            _endpointsStorage = endpointsStorage;
            _responsePackageMonitor = responsePackageMonitor;
            _sendingMiddlewaresRunner = sendingMiddlewaresRunner;
            _endpointsInvoker = endpointsInvoker;
            _logger = logger;
        }

        public Package? Send(Package package)
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

            if (endpoint.EndpointType is EndpointType.Event)
                throw new FatNetLibException("Cannot call event-endpoint over the network");

            package.Reliability = endpoint.Reliability;
            if ((endpoint.EndpointType is EndpointType.Exchanger || endpoint.EndpointType is EndpointType.Initial) &&
                package.ExchangeId == Guid.Empty)
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
                EndpointType.Initial => _responsePackageMonitor.Wait(package.ExchangeId),
                _ => throw new FatNetLibException($"Unsupported {nameof(EndpointType)} {endpoint.EndpointType}")
            };
        }

        public void EmitEvent(Package package)
        {
            if (package is null) throw new ArgumentNullException(nameof(package));
            if (package.Route is null) throw new ArgumentNullException(nameof(package.Route));

            IEnumerable<LocalEndpoint> endpoints = _endpointsStorage.LocalEndpoints
                .Where(_ => _.EndpointData.Route.Equals(package.Route)).ToList();

            if (!endpoints.Any())
                _logger.Debug($"No event-endpoints registered with route {package.Route}");

            if (endpoints.Any(_ => _.EndpointData.EndpointType != EndpointType.Event))
                throw new FatNetLibException("Cannot emit not event endpoint");

            foreach (LocalEndpoint endpoint in endpoints)
            {
                _endpointsInvoker.InvokeReceiver(endpoint, package);
            }
        }
    }
}
