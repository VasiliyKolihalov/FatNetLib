# Serialization

In order to deliver a package over the network, it must be serialized that is turned into an array of bytes.
The receiving side needs to restore the package from the byte array, i.e., deserialize.
In FatNetLib, serialization and deserialization take place in middleware.
This allows you to customize the format that is necessary.
FatNetLib also provides an assembly for json serialization - `FatNetLib.Json`.

## How a package is serialized

When we send a package using `Courier`, it goes through `SendingMiddlewares` which should include middleware responsible
for serialization. Within this middleware, all sending fields of the package are serialized into a sequence of bytes
according to the serialization format. They are then put into the `Serialized` field of the package and delivered over
the network. On the receiving side, an empty package is created with the `Serialized` field set. Next, the package goes
through `ReceivingMiddlewares`, which must include the deserialization middleware. Inside this middleware based on
package schema the value of the `Serialized` field is deserialized into the fields of the package according to the
format.

The middleware of serialization and deserialization will work in pairs.
If a package has come to the deserialization middleware that does not match its scheme,
then middleware should throw an exception.

## Working with `FatNetLib.Json`

`FatNetLib.Json` provides a `JsonModule` module for json serialization. It is suitable for both the server and the
client.

```c#
var builder = new FatNetLibBuilder
{
     Modules=
     {
         new JsonModule(),
         // ...
     }
};
```

`JsonModule` is based on the `Newtonsoft.Json` library. It adds one SendingMiddleware - `JsonSerializationMiddleware`
and one ReceivingMiddleware - `JsonDeserializationMiddleware`. It also adds three standard
converters: `RouteConverter`, `TypeConverter` and `PackageSchemaConverter`.

If you have a problem with the serialization and deserialization of a type, you need to create a converter for this type
and a module that will add this converter.

```c#
using static Kolyhalov.FatNetLib.Core.Modules.ModuleId;
using static Kolyhalov.FatNetLib.Core.Modules.ModuleId.Pointers;
using static Kolyhalov.FatNetLib.Core.Modules.Steps.StepType;

public class CustomConvertersModule : IModule
{
     public void Setup(IModuleContext moduleContext)
     {
         moduleContext.FindStep(
                parent: ThisModule,
                step: PutScript,
                qualifier: "AddConverter")
             .AndMoveAfterStep(
                parent: RootModule / typeof(JsonModule),
                step: PutDependency,
                qualifier: typeof(IList<JsonConverter>))
             .PutScript("AddMyConverter", _ => _.Get<IList<JsonConverter>>().Add(new MyConverter()));
     }
}
```

This module must be placed before `JsonModule`:

```c#
var builder = new FatNetLibBuilder
{
     modules=
     {
         // ...
         new CustomConvertersModule(),
         new JsonModule()
     }
};
```

When using `JsonModule` make sure you set the [middlewares order](6-middlewares.md) correctly.