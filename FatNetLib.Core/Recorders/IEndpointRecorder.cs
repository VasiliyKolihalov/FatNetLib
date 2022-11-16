using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Controllers;
using Kolyhalov.FatNetLib.Core.Delegates;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Models;

namespace Kolyhalov.FatNetLib.Core.Recorders
{
    public interface IEndpointRecorder
    {
        public IEndpointRecorder AddController(IController controller);

        public IEndpointRecorder AddReceiver(
            Route route,
            ReceiverDelegate receiverDelegate,
            Reliability reliability = Reliability.ReliableOrdered,
            PackageSchema? requestSchemaPatch = default);

        public IEndpointRecorder AddReceiver(
            string route,
            ReceiverDelegate receiverDelegate,
            Reliability reliability = Reliability.ReliableOrdered,
            PackageSchema? requestSchemaPatch = default);

        public IEndpointRecorder AddExchanger(
            Route route,
            ExchangerDelegate exchangerDelegate,
            Reliability reliability = Reliability.ReliableOrdered,
            PackageSchema? requestSchemaPatch = default,
            PackageSchema? responseSchemaPatch = default);

        public IEndpointRecorder AddExchanger(
            string route,
            ExchangerDelegate exchangerDelegate,
            Reliability reliability = Reliability.ReliableOrdered,
            PackageSchema? requestSchemaPatch = default,
            PackageSchema? responseSchemaPatch = default);

        public IEndpointRecorder AddInitial(
            string route,
            ExchangerDelegate exchangerDelegate,
            PackageSchema? requestSchemaPatch = default,
            PackageSchema? responseSchemaPatch = default);

        public IEndpointRecorder AddInitial(
            Route route,
            ExchangerDelegate exchangerDelegate,
            PackageSchema? requestSchemaPatch = default,
            PackageSchema? responseSchemaPatch = default);
    }
}
