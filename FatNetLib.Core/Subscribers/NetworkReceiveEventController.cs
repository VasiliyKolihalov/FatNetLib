using System;
using System.Linq;
using Kolyhalov.FatNetLib.Core.Attributes;
using Kolyhalov.FatNetLib.Core.Components;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Controllers;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Monitors;
using Kolyhalov.FatNetLib.Core.Runners;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Utils;
using Kolyhalov.FatNetLib.Core.Wrappers;
using LiteNetLib.Utils;
using static Kolyhalov.FatNetLib.Core.Constants.RouteConstants.Strings.Events;

namespace Kolyhalov.FatNetLib.Core.Subscribers
{
    public class NetworkReceiveEventController : IController
    {
        private readonly IResponsePackageMonitor _responsePackageMonitor;
        private readonly IMiddlewaresRunner _receivingMiddlewaresRunner;
        private readonly PackageSchema _defaultPackageSchema;
        private readonly IDependencyContext _context;
        private readonly IEndpointsStorage _endpointsStorage;
        private readonly IEndpointsInvoker _endpointsInvoker;
        private readonly IMiddlewaresRunner _sendingMiddlewaresRunner;

        public NetworkReceiveEventController(
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

        [EventListener]
        [Route(NetworkReceived)]
        public void Handle(Package package)
        {
            var body = package.GetBodyAs<NetworkReceiveBody>();
            Handle(body.Peer, body.DataReader, body.Reliability);
        }

        private void Handle(INetPeer peer, NetDataReader reader, Reliability reliability)
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
                case EndpointType.Consumer:
                    HandleConsumer(endpoint, receivedPackage);
                    return;
                case EndpointType.Exchanger:
                    HandleExchanger(endpoint, receivedPackage);
                    return;
                case EndpointType.Initializer:
                    HandleExchanger(endpoint, receivedPackage);
                    break;
                case EndpointType.EventListener:
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
                Sender = peer,
                Reliability = reliability
            };
        }

        private LocalEndpoint GetEndpoint(Package requestPackage)
        {
            LocalEndpoint endpoint =
                _endpointsStorage.LocalEndpoints
                    .FirstOrDefault(_ => _.Details.Route.Equals(requestPackage.Route))
                ?? throw new FatNetLibException($"Package from peer {requestPackage.Sender!.Id} " +
                                                $"pointed to a non-existent endpoint. Route: {requestPackage.Route}");

            if (endpoint.Details.Reliability != requestPackage.Reliability)
                throw new FatNetLibException(
                    $"Package from {requestPackage.Sender!.Id} came with the wrong type of reliability");

            return endpoint;
        }

        private void HandleConsumer(LocalEndpoint endpoint, Package requestPackage)
        {
            _endpointsInvoker.InvokeConsumer(endpoint, requestPackage);
        }

        private void HandleExchanger(LocalEndpoint endpoint, Package requestPackage)
        {
            Package packageToSend = _endpointsInvoker.InvokeExchanger(endpoint, requestPackage);

            packageToSend.Route = requestPackage.Route;
            packageToSend.ExchangeId = requestPackage.ExchangeId;
            packageToSend.IsResponse = true;
            packageToSend.Context = _context;
            packageToSend.Receiver = requestPackage.Sender;
            packageToSend.Reliability = requestPackage.Reliability;
            HandlePossibleInvocationException(packageToSend);

            _sendingMiddlewaresRunner.Process(packageToSend);

            (packageToSend.Receiver as ISendingNetPeer)!.Send(packageToSend);
        }

        private static void HandlePossibleInvocationException(Package packageToSend)
        {
            var invocationException = packageToSend.GetNonSendingField<Exception?>("InvocationException");
            if (invocationException == null)
                return;

            packageToSend.Error = invocationException.ToEndpointRunFailedView();
            var newPatch = new PackageSchema { { nameof(Package.Error), typeof(EndpointRunFailedView) } };
            packageToSend.ApplySchemaPatch(newPatch);
        }
    }
}
