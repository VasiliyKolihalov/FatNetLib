using LiteNetLib;

namespace Kolyhalov.FatNetLib;

public interface IEndpointRecorder
{
    public void AddController(IController controller);
    public void AddReceiver(string route, DeliveryMethod deliveryMethod, ReceiverDelegate receiverDelegate);
    public void AddExchanger(string route, DeliveryMethod deliveryMethod, ExchangerDelegate exchangerDelegate);
}