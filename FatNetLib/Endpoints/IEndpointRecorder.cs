using Kolyhalov.FatNetLib.Microtypes;
using LiteNetLib;

namespace Kolyhalov.FatNetLib.Endpoints;

public interface IEndpointRecorder
{
    public IEndpointRecorder AddController(IController controller);

    public IEndpointRecorder AddReceiver(
        Route route,
        DeliveryMethod deliveryMethod,
        ReceiverDelegate receiverDelegate,
        PackageSchema? requestSchemaPatch = default);

    public IEndpointRecorder AddReceiver(
        string route,
        DeliveryMethod deliveryMethod,
        ReceiverDelegate receiverDelegate,
        PackageSchema? requestSchemaPatch = default);

    public IEndpointRecorder AddExchanger(
        Route route,
        DeliveryMethod deliveryMethod,
        ExchangerDelegate exchangerDelegate,
        PackageSchema? requestSchemaPatch = default,
        PackageSchema? responseSchemaPatch = default);

    public IEndpointRecorder AddExchanger(
        string route,
        DeliveryMethod deliveryMethod,
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
