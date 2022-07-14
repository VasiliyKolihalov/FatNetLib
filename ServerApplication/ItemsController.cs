using Framework;
using LiteNetLib;

namespace ServerApplication;

[Route("api/Items")]
public class ItemsController : IController
{
    private static ItemsService _itemsService;
    
    public ItemsController(ItemsService itemsService)
    {
        _itemsService = itemsService;
    }
    
    [Route("get-all")]
    [Sender]
    public Package GetAll()
    {
        IEnumerable<Item> items = _itemsService.GetAll();
        return new Package()
        {
            Body = new Dictionary<string, object>() {["Items"] = items}
        };
    }
    
    [Route("add-item")]
    [Receiver(DeliveryMethod.Unreliable)]
    public void Add(Item item)
    {
        _itemsService.Add(item);
    }

}