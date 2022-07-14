using Framework;

namespace ServerApplication;

public class ItemsService
{
    private readonly List<Item> _items;
    private readonly UdpFrameworkClient _udpFrameworkClient;

    public ItemsService(UdpFrameworkClient udpFrameworkClient)
    {
        _items = new List<Item>();
        _udpFrameworkClient = udpFrameworkClient;
    }

    public IEnumerable<Item> GetAll()
    {
        return _items.ToArray();
    }

    public void Add(Item item)
    {
        _items.Add(item);
        _udpFrameworkClient.Send("items/get-all");
    }
}