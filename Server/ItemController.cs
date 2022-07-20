using LiteNetLib;
using UdpFramework;

namespace Server;

public class ItemController : IController
{
    private readonly UdpFramework.UdpFramework _udpFramework;
    private readonly List<Item> _items = new List<Item>();

    public ItemController(UdpFramework.UdpFramework udpFramework)
    {
        _udpFramework = udpFramework;
    }

    [Route("add-item")]
    [Receiver(DeliveryMethod.Sequenced)]
    public void Add(Item item)
    {
        _items.Add(item);
        Package package = new Package()
        {
            Route = "/get-items",
            Body = new Dictionary<string, object>
            {
                ["Items"] = _items
            }
        };
        _udpFramework.SendRequest(package, 0);
    }
}