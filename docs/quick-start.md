# Быстрый старт

## Инициализация
```c#
new UpdFraemworkBuilder()
    .AddMiddleware(new BarMiddleware())
    .AddController(new FooController())
    .Build()
    .Run(); 
```
## Контроллер
```c#
[Route("Foo")]
public class FooController : Controller
{
    private readonly FooService _fooService;
    
    public ItemsController(FooService fooService) => _fooService = fooService;
    
    [Route("{fooId}/Get")]
    [Sender]
    public PackageBuilder Get(int fooId)
    {
        Foo foo = _fooService.Get(fooId)
        return new PackageBuilder()
        {
            Body = foo
        };
    }
    
    [Route("Add")]
    [Receiver(DeliveryMethod.Unreliable)]
    public void Add([FromBody] Foo foo)
    {
        _fooService.Add(foo);
    }
    
    [Route("Update")]
    [Exchanger(DeliveryMethod.ReliableUnordered)]
    public PackageBuilder Update([FromBody] Foo foo, [FromPackage("Bar")] Bar bar)
    {
        _fooService.Update(foo);
        return new PackageBuilder()
        {
            Body = new Foo(),   
            Customs["Bar"] = bar
        };
    }
}
```
- `Route` - атрибут маршутизации.
- `Controller` - абстрактный класс, от которого необходимо наследоваться, если вы хотите создать контроллер.
- `PackageBuilder` - тип данных необходимый для ответа. Имеет стандартные для протокола поля типа "Body", "Auth" и т.д.
Для кастомных полей имеется словарь Customs.
- `Sender` - атрибут, который указывает, что эндпоинт будет "отпралять" пакет.
- `Receiver` - атрибут, который указывает, что эндпоинт будет "принимать" пакет.
Принимает в констроктур енум `DeliveryMethod`, отвечающий за гарантии доставки.
- `Exchanger` - атрибут, который указывает, что эндпоинт будет принимать и отправлять пакетами.
  Принимает в констроктур енум `DeliveryMethod`, отвечающий за гарантии доставки. 
- `FromBody` - атрибут, указываюший на то, что параметр эндпоинта будет десериализоваться из сообщения. 
Помимо `FromBody` доступны атрибуты для все стандартных полей протокола, например `FromAuth`. Для кастомных полей, есть атрибут
`[FromPackage]`, который принимает имя кастомного поля.