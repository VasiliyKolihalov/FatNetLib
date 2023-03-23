﻿using System;
using System.Collections.Generic;
using System.Linq;
using Kolyhalov.FatNetLib.Core.Components;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Monitors;
using Kolyhalov.FatNetLib.Core.Runners;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core.Couriers
{
    public abstract class Courier : ICourier
    {
        protected readonly IList<INetPeer> ConnectedPeers;
        private readonly IEndpointsStorage _endpointsStorage;
        private readonly IResponsePackageMonitor _responsePackageMonitor;
        private readonly IMiddlewaresRunner _sendingMiddlewaresRunner;
        private readonly IEndpointsInvoker _endpointsInvoker;
        private readonly ILogger _logger;

        protected Courier(
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

        public IList<INetPeer> Peers => ConnectedPeers;

        public Package? Send(Package package)
        {
            if (package is null) throw new ArgumentNullException(nameof(package));

            if (package.Route is null) throw new ArgumentNullException(nameof(package.Route));

            if (package.Fields.ContainsKey(nameof(Package.IsResponse)) && package.IsResponse)
                throw new FatNetLibException("Sending response packages is not allowed");
            package.IsResponse = false;

            ISendingNetPeer toPeer = package.ToPeer as ISendingNetPeer
                                     ?? throw new ArgumentNullException(nameof(package.ToPeer));

            Endpoint endpoint = _endpointsStorage.RemoteEndpoints[toPeer.Id]
                                    .FirstOrDefault(endpoint => endpoint.Route.Equals(package.Route)) ??
                                throw new FatNetLibException("Remote endpoint not found");

            if (endpoint.Type is EndpointType.Event)
                throw new FatNetLibException("Cannot call event endpoint over the network");

            package.Reliability = endpoint.Reliability;
            if (NeedToGenerateGuid(endpoint, package))
            {
                package.ExchangeId = Guid.NewGuid();
            }

            _sendingMiddlewaresRunner.Process(package);

            toPeer.Send(package);

            return endpoint.Type switch
            {
                EndpointType.Receiver => null,
                EndpointType.Exchanger =>
                    HandleErrorResponse(_responsePackageMonitor.Wait(package.ExchangeId!.Value)),
                EndpointType.Initializer =>
                    HandleErrorResponse(_responsePackageMonitor.Wait(package.ExchangeId!.Value)),
                _ => throw new FatNetLibException($"Unsupported {nameof(EndpointType)} {endpoint.Type}")
            };
        }

        private static bool NeedToGenerateGuid(Endpoint endpoint, Package package)
        {
            return (endpoint.Type is EndpointType.Exchanger || endpoint.Type is EndpointType.Initializer)
                   && !package.Fields.ContainsKey(nameof(Package.ExchangeId));
        }

        private static Package HandleErrorResponse(Package response)
        {
            return response.Error is null
                ? response
                : throw new ErrorResponseFatNetLibException("Peer responded with error", response);
        }

        public void EmitEvent(Package package)
        {
            if (package is null) throw new ArgumentNullException(nameof(package));
            if (package.Route is null) throw new ArgumentNullException(nameof(package.Route));

            IEnumerable<LocalEndpoint> endpoints = _endpointsStorage.LocalEndpoints
                .Where(_ => _.Details.Route.Equals(package.Route)).ToList();

            if (!endpoints.Any())
                _logger.Debug($"No event endpoints registered with route {package.Route}");

            if (endpoints.Any(_ => _.Details.Type != EndpointType.Event))
                throw new FatNetLibException("Cannot emit not event endpoint");

            foreach (LocalEndpoint endpoint in endpoints)
            {
                _endpointsInvoker.InvokeReceiver(endpoint, package);
            }
        }
    }
}
