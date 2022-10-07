using Kolyhalov.FatNetLib.Microtypes;
using LiteNetLib;

namespace Kolyhalov.FatNetLib.Endpoints;

public interface IEndpointRecorder
{
    public void AddController(IController controller);
    public void AddReceiver(Route route, DeliveryMethod deliveryMethod, ReceiverDelegate receiverDelegate);
    public void AddReceiver(string route, DeliveryMethod deliveryMethod, ReceiverDelegate receiverDelegate);
    public void AddExchanger(Route route, DeliveryMethod deliveryMethod, ExchangerDelegate exchangerDelegate);
    public void AddExchanger(string route, DeliveryMethod deliveryMethod, ExchangerDelegate exchangerDelegate);
}