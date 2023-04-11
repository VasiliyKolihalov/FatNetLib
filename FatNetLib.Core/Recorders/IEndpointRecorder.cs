using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Controllers;
using Kolyhalov.FatNetLib.Core.Delegates;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Models;

namespace Kolyhalov.FatNetLib.Core.Recorders
{
    public interface IEndpointRecorder
    {
        public IEndpointRecorder AddConsumer(
            Route route,
            ConsumerAction action,
            Reliability reliability = Reliability.ReliableOrdered,
            PackageSchema? requestSchemaPatch = default);

        public IEndpointRecorder AddExchanger(
            Route route,
            ExchangerAction action,
            Reliability reliability = Reliability.ReliableOrdered,
            PackageSchema? requestSchemaPatch = default,
            PackageSchema? responseSchemaPatch = default);

        public IEndpointRecorder AddInitial(
            Route route,
            ExchangerAction action,
            PackageSchema? requestSchemaPatch = default,
            PackageSchema? responseSchemaPatch = default);

        public IEndpointRecorder AddEventListener(Route route, EventAction action);

        public IEndpointRecorder AddController(IController controller);
    }
}
