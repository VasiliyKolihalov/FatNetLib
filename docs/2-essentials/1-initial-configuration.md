# Initial configuration

For setup and run framework, you need to use  `FatNetLibBuilder`. It is responsible for:

* Register modules
* Patch configuration
* Patch DefaultPackageSchema
* Set the order of middlewares
* Register endpoints
* Create and run `FatNetLib`

## Module registration

Modules are components that are primarily responsible for assembly and configuration of the framework.

To register modules, use the `Module` property.

```c#
var builder = new FatNetLibBuilder
{
    Modules =
    {
        new DefaultClientModule()
    }
    // ...
};
```

There are two types of applications: **Server** and **Client**.
You should define an application type by registering proper modules.
There are a couple of standard modules, for the server use - `DefaultServerModule` and for the client
use `DefaultClientModule`.

Modules allow you to customize the framework much deeper than `FatNetLibBuilder`.
You can create your own modules to build the framework.
Learn more about [modules](10-modules.md).

## Configuration Patch

The `DefaultServerModule` and `DefaultClientModule` modules set default configuration values. They look like:

* `Port` - 2812
* `Framerate` - 60
* `ExchangeTimeout` - 10 sec.
* For server: `MaxPeers` - int.MaxValue
* For client: `Address` - localhost

To patch them, use the `ConfigurationPatch` property. Two types of values are passed to it: `ServerConfiguration`
and `ClientConfiguration`. Choose the type of configuration according to the type of your application.

```c#
var builder = new FatNetLibBuilder
{
    Modules=
    {
        new DefaultClientModule(),
        // ...
    },
    ConfigurationPatch = new ClientConfiguration
    {
        Port = new Port(1112),
        ExchangeTimeout = TimeSpan.FromSeconds(5)
    }
    // ...
};
```

## DefaultPackageSchema Patch

PackageSchema is a type refinement to deserialize package fields. Default modules set default package schema. It looks
like:

```c#
new PackageSchema
{
    { nameof(Package.Route), typeof(Route) },
    { nameof(Package.Body), typeof(IDictionary<string, object>) },
    { nameof(Package.ExchangeId), typeof(Guid) },
    { nameof(Package.IsResponse), typeof(bool) },
    { nameof(Package.SchemaPatch), typeof(PackageSchema) }
});
```

To patch it, you need to use the `DefaultPackageSchemaPatch` field.

```c#
var builder = new FatNetLibBuilder
{
    // ...
    DefaultPackageSchemaPatch = new PackageSchema
    {
        {"PackageField" : typeof(MyOwnType)}
    });
};
```

Learn more about [PackageSchema](4-package-schema.md).

## Middleware order

Typically, when modules register middlewares, they line up in the wrong order.
To set the correct order, you need to use the `SendingMiddlewaresOrder` and `ReceivingMiddlewaresOrder` properties.

```c#
new FatNetLibBuilder
{
    Modules=
    {
        new DefaultServerModule(),
        new JsonModule()
    },
    SendingMiddlewaresOrder = new[]
    {
        typeof(JsonSerializationMiddleware),
        typeof(EncryptionMiddleware)
    },
    ReceivingMiddlewaresOrder = new[]
    {
        typeof(DecryptionMiddleware),
        typeof(JsonDeserializationMiddleware)
    }
};
```

Learn more about [Middlewares](6-middlewares.md).

## Endpoint registration

To register endpoints, you need to use the `Endpoints` property.

```c#
builder.Endpoints.AddReceiver(
    new Route("items/show"),
    package =>
    {
        // ...
    });

builder.Endpoints.AddController(new MyController());
```

Learn more about [Endpoints](2-endpoints.md).

## FatNetLib creation

The `BuildAndRun()` method creates, runs and returns an instance of `FatNetLib`.
You can save it for access to `Courier` and to stop the framework:

```c#
var builder = new FatNetLibBuilders
{
    // ...
};
FatNetLib fatNetLib = buider.BuildAndRun();
ICourier courier = fatNetLib.Courier;

// ...

fatNetLib.Stop();
```

Learn more about [Courier](5-courier.md).

After calling `BuildAndRun()` method, all other setup actions with the builder will not make sense.

