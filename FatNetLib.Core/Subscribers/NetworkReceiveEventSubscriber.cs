﻿using System.Linq;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Monitors;
using Kolyhalov.FatNetLib.Core.Runners;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Wrappers;
using LiteNetLib.Utils;

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

        public NetworkReceiveEventSubscriber(
            IResponsePackageMonitor responsePackageMonitor,
            IMiddlewaresRunner receivingMiddlewaresRunner,
            PackageSchema defaultPackageSchema,
            IDependencyContext context,
            IEndpointsStorage endpointsStorage,
            IEndpointsInvoker endpointsInvoker,
            IMiddlewaresRunner sendingMiddlewaresRunner)
        {
            _responsePackageMonitor = responsePackageMonitor;
            _receivingMiddlewaresRunner = receivingMiddlewaresRunner;
            _defaultPackageSchema = defaultPackageSchema;
            _context = context;
            _endpointsStorage = endpointsStorage;
            _endpointsInvoker = endpointsInvoker;
            _sendingMiddlewaresRunner = sendingMiddlewaresRunner;
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

            switch (endpoint.Details.Type)
            {
                case EndpointType.Receiver:
                    HandleReceiver(endpoint, receivedPackage);
                    return;
                case EndpointType.Exchanger:
                    HandleExchanger(endpoint, receivedPackage);
                    return;
                case EndpointType.Initializer:
                    HandleExchanger(endpoint, receivedPackage);
                    break;
                case EndpointType.Event:
                default:
                    throw new FatNetLibException(
                        $"{endpoint.Details.Type} is not supported in NetworkReceiveEventSubscriber");
            }
        }

        private Package BuildReceivedPackage(INetPeer peer, NetDataReader reader, Reliability reliability)
        {
            return new Package
            {
                Serialized = reader.GetRemainingBytes(),
                Schema = new PackageSchema(_defaultPackageSchema),
                Context = _context,
                FromPeer = peer,
                Reliability = reliability
            };
        }

        private LocalEndpoint GetEndpoint(Package requestPackage)
        {
            LocalEndpoint endpoint =
                _endpointsStorage.LocalEndpoints
                    .FirstOrDefault(_ => _.Details.Route.Equals(requestPackage.Route))
                ?? throw new FatNetLibException($"Package from peer {requestPackage.FromPeer!.Id} " +
                                                $"pointed to a non-existent endpoint. Route: {requestPackage.Route}");

            if (endpoint.Details.Reliability != requestPackage.Reliability)
                throw new FatNetLibException(
                    $"Package from {requestPackage.FromPeer!.Id} came with the wrong type of reliability");

            return endpoint;
        }

        private void HandleReceiver(LocalEndpoint endpoint, Package requestPackage)
        {
            _endpointsInvoker.InvokeReceiver(endpoint, requestPackage);
        }

        private void HandleExchanger(LocalEndpoint endpoint, Package requestPackage)
        {
            Package packageToSend = _endpointsInvoker.InvokeExchanger(endpoint, requestPackage);

            packageToSend.Route = requestPackage.Route;
            packageToSend.ExchangeId = requestPackage.ExchangeId;
            packageToSend.IsResponse = true;
            packageToSend.Context = _context;
            packageToSend.ToPeer = requestPackage.FromPeer;
            packageToSend.Reliability = requestPackage.Reliability;

            _sendingMiddlewaresRunner.Process(packageToSend);

            (packageToSend.ToPeer as ISendingNetPeer)!.Send(packageToSend);
        }
    }
}
