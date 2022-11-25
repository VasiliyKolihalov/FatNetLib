using System;
using System.Collections.Generic;
using System.Linq;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Monitors;
using Kolyhalov.FatNetLib.Core.Runners;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Wrappers;
using LiteNetLib.Utils;
using static Kolyhalov.FatNetLib.Core.Controllers.RouteConstants.Routes.Events;

namespace Kolyhalov.FatNetLib.Core.Subscribers
{
    public class NetworkReceiveEventSubscriber : INetworkReceiveEventSubscriber
    {
        private readonly IResponsePackageMonitor _responsePackageMonitor;
        private readonly IMiddlewaresRunner _receivingMiddlewaresRunner;
        private readonly PackageSchema _defaultPackageSchema;
        private readonly IDependencyContext _context;
        private readonly IEndpointsStorage _endpointsStorage;
        private readonly IEndpointsInvoker _endpointsInvoker;
        private readonly IMiddlewaresRunner _sendingMiddlewaresRunner;
        private readonly IList<INetPeer> _connectedPeers;
        private readonly Route _lastInitializerRoute;
        private readonly ICourier _courier;

        public NetworkReceiveEventSubscriber(
            IResponsePackageMonitor responsePackageMonitor,
            IMiddlewaresRunner receivingMiddlewaresRunner,
            PackageSchema defaultPackageSchema,
            IDependencyContext context,
            IEndpointsStorage endpointsStorage,
            IEndpointsInvoker endpointsInvoker,
            IMiddlewaresRunner sendingMiddlewaresRunner,
            IList<INetPeer> connectedPeers,
            Route lastInitializerRoute,
            ICourier courier)
        {
            if (lastInitializerRoute.Equals(Route.Empty))
                throw new NotImplementedException("Connections without initializers are not supported yet");

            _responsePackageMonitor = responsePackageMonitor;
            _receivingMiddlewaresRunner = receivingMiddlewaresRunner;
            _defaultPackageSchema = defaultPackageSchema;
            _context = context;
            _endpointsStorage = endpointsStorage;
            _endpointsInvoker = endpointsInvoker;
            _sendingMiddlewaresRunner = sendingMiddlewaresRunner;
            _connectedPeers = connectedPeers;
            _lastInitializerRoute = lastInitializerRoute;
            _courier = courier;
        }

        public void Handle(INetPeer peer, NetDataReader reader, Reliability reliability)
        {
            Package receivedPackage = BuildReceivedPackage(peer, reader, reliability);

            _receivingMiddlewaresRunner.Process(receivedPackage);

            if (receivedPackage.IsResponse)
            {
                _responsePackageMonitor.Pulse(receivedPackage);
                return;
            }

            LocalEndpoint endpoint = GetEndpoint(receivedPackage);

            switch (endpoint.EndpointData.EndpointType)
            {
                case EndpointType.Receiver:
                    _endpointsInvoker.InvokeReceiver(endpoint, receivedPackage);
                    return;
                case EndpointType.Exchanger:
                    HandleExchanger(endpoint, receivedPackage);
                    return;
                case EndpointType.Initial:
                    HandleExchanger(endpoint, receivedPackage);
                    break;
                default:
                    throw new FatNetLibException(
                        $"{endpoint.EndpointData.EndpointType} is not supported in NetworkReceiveEventSubscriber");
            }

            if (endpoint.EndpointData.Route.Equals(_lastInitializerRoute))
            {
                _courier.EmitEvent(new Package
                {
                    Route = InitializationFinished,
                    Body = peer
                });
            }
        }

        private Package BuildReceivedPackage(INetPeer peer, NetDataReader reader, Reliability reliability)
        {
            return new Package
            {
                Serialized = reader.GetRemainingBytes(),
                Schema = new PackageSchema(_defaultPackageSchema),
                Context = _context,
                FromPeerId = peer.Id,
                Reliability = reliability
            };
        }

        private LocalEndpoint GetEndpoint(Package requestPackage)
        {
            LocalEndpoint endpoint =
                _endpointsStorage.LocalEndpoints
                    .FirstOrDefault(_ => _.EndpointData.Route.Equals(requestPackage.Route))
                ?? throw new FatNetLibException($"Package from {requestPackage.FromPeerId} " +
                                                $"pointed to a non-existent endpoint. Route: {requestPackage.Route}");

            if (endpoint.EndpointData.Reliability != requestPackage.Reliability)
                throw new FatNetLibException(
                    $"Package from {requestPackage.FromPeerId} came with the wrong type of reliability");

            return endpoint;
        }

        private void HandleExchanger(LocalEndpoint endpoint, Package requestPackage)
        {
            Package packageToSend = _endpointsInvoker.InvokeExchanger(endpoint, requestPackage);

            packageToSend.Route = requestPackage.Route;
            packageToSend.ExchangeId = requestPackage.ExchangeId;
            packageToSend.IsResponse = true;
            packageToSend.Context = _context;
            packageToSend.ToPeerId = requestPackage.FromPeerId;
            packageToSend.Reliability = requestPackage.Reliability;

            _sendingMiddlewaresRunner.Process(packageToSend);

            _connectedPeers.Single(netPeer => netPeer.Id == packageToSend.ToPeerId)
                .Send(packageToSend);
        }
    }
}
