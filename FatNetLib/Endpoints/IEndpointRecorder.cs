using Kolyhalov.FatNetLib.Microtypes;

namespace Kolyhalov.FatNetLib.Endpoints;

public interface IEndpointRecorder
{
    public IEndpointRecorder AddController(IController controller);

    public IEndpointRecorder AddReceiver(
        Route route,
        Reliability reliability,
        ReceiverDelegate receiverDelegate,
        PackageSchema? requestSchemaPatch = default);

    public IEndpointRecorder AddReceiver(
        string route,
        Reliability reliability,
        ReceiverDelegate receiverDelegate,
        PackageSchema? requestSchemaPatch = default);

    public IEndpointRecorder AddExchanger(
        Route route,
        Reliability reliability,
        ExchangerDelegate exchangerDelegate,
        PackageSchema? requestSchemaPatch = default,
        PackageSchema? responseSchemaPatch = default);

    public IEndpointRecorder AddExchanger(
        string route,
        Reliability reliability,
        ExchangerDelegate exchangerDelegate,
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
