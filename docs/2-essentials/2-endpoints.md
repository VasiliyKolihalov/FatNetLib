# Endpoints

An endpoint is delivery interface designed for transferring packages over the network or locally.
By publishing endpoints receiving side declares its readiness to process incoming packages using a specific handler.
Every endpoint has its own route — you can treat it like an address.
So if sending side wants to send a package to a specific endpoint on the receiving side,
it has to include endpoint's route to the package.

There are the following types of endpoints:

* Consumer — simple network endpoint that handles an incoming package and does not return a response.

* Exchanger — network endpoint that processes a handle package and always returns a response package.

* Initializer — network endpoint for initial connection setup.
  Not intended for business logic.
  Called automatically by the client on the server immediately after the start.
  The first segment of the route must be `fat-net-lib`.
  Always returns a response package.
  Has the highest delivery guarantee.

* EventListener — local endpoint looks like C# events.
  It сan has multiple handlers on the same route.
  Does not return a response package.

## Routing

Routing is responsible for matching incoming packages to routes and selecting an endpoint for handling.
Route is an endpoint identifier.
The `Route` class is used to define a route.
It is an ordered set of **segments**, separated by `/`.
Segments are the strings that make up the route.

In route segments, you can use:

* Unicode letters
* Digits
* Most special characters on the keyboard

The `/` character is a segment separator and cannot be part of it.
Note that the `\ ` character is intentionally removed from route alphabet.

The route is not designed to store data and has no alternatives to query parameters from HTTP.
Also unlike HTTP, FatNetLib does not support URL encoding.

We recommend using kebab-case to describe route segments.
Since there are no fixed verbs in FatNetLib, as in HTTP, you can add a verb to any segment, or omit it completely.

## Registration

There are two styles of registering endpoints: Builder-style and Controller-style. They have the same functionality.

To register endpoints, you need to use the methods of the `IEndpointRecorder` class available from `FatNetLibBuider`.

To register an endpoint in Builder-style, use one of the following methods:

* `AddConsumer()`
* `AddExchanger()`
* `AddInitializer()`
* `AddEventListener()`

```c#
builder.Endpoints.AddConsumer(
     new Route("items/add"),
     action: async package =>
     {
         // ...
     });
```

To register endpoints in Controller-style, use the `AddController()` method. You can also
use the `PutController()` method from the module. Read more about [Registering endpoints in modules](10-modules.md).

These methods accept a controller instance. A controller is a class that describes endpoints.
To create controller, you must implement the empty `IController` interface.

```c#
builder.Endpoints.AddController(new ItemsController());

[Route("items")]
class ItemsController : IController
{
     [Consumer]
     [Route("Add")]
     public Task AddAsync(Package package)
     {
         // ...
     }
}
```

`FatNetLib` supports synchronous and asynchronous endpoints.

### Endpoint attributes

When registering endpoints, set the following parameters:

* Endpoint type. In Builder-style, the endpoint type determines the method by which you register it.
  `IEndpointRecorder` has a method for each type: `AddConsumer`, `AddExchanger`, `AddInitial` and `AddEventListener`. In
  controller-style uses attributes `[Consumer]`, `[Exchanger]`, `[Initializer]`
  and `[EventListener]`. These attributes are only available to methods and required.

* Route — endpoint identifier.
  When registering with Builder-style, `Route` is passed as an argument.
  In Controller-style there is a `[Route]` attribute.
  Route is available for all types of endpoints and required.

* Reliability — guarantee of package delivery to the endpoint.
  When registering with Builder-style, `Reliability` is passed as argument.
  In Controller-style, `Reliability` is passed to the constructor of the attribute that defines the type endpoint.
  The highest guarantee is set by default
  `Reliability` can be set for all network endpoints, except Initial endpoints.
  They always have the highest guarantee.

* PackageSchemaPatch — allows you to specify the types of fields in the
  package for deserialization. More often, the type Body is specified. Set separately for the incoming and for the
  response package (`RequestSchemaPatch`
  and `ResponseSchemaPatch`). When registering with Builder-style, it is passed as an argument. And in Controller-style
  there is the `[Schema]` attribute. PackageSchemaPatch can be set for all network endpoints. Learn more
  about [PackageSchema](4-package-schema.md).

## Reliability

Reliability is a guarantee of package delivery over the network.

When working with Reliability types, the following concepts are encountered:

* Package drop — whether packages will reach the endpoint over the network or not.
* Ordering — whether packages will arrive, according to the sending queue.
* Duplication — whether packages will multiply during delivery.
* Fragmentation - whether packages will be split up to be sent over the network. Note that if the Reliability type does
  not support fragmentation, the maximum package size is limited by MTU.

The following Reliability types exist:

* ReliableOrdered — Ensures orderly delivery of packages.
  Packages will not be dropped or duplicated.
  Most reliable type of delivery.
* ReliableUnordered — Guarantees delivery of packages.
  Packages will not be dropped or duplicated, but may arrive out of turn.
* ReliableSequenced — Ensures that only the last package is delivered.
  Packages can be dropped (except the last one) and will not be duplicated.
  Packages will arrive in order and cannot be fragmented.
* Sequenced — Ensures ordered delivery of packages. Packages can be dropped and will not be duplicated.
* Unreliable — Does not guarantee delivery of packages.
  Packages can be dropped, duplicated, and arrived out of order.
  packages cannot be fragmented. Reminds a standard UDP package.

|                         | ReliableOrdered | ReliableUnordered |   ReliableSequenced   | Sequenced | Unreliable |
|:-----------------------:|:---------------:|:-----------------:|:---------------------:|:---------:|:----------:|
| Package drop prevention |        ✅        |         ✅         | **Only last package** |     ❌     |     ❌      |
|        Ordering         |        ✅        |         ❌         |           ✅           |     ✅     |     ❌      |
| Duplication prevention  |        ✅        |         ✅         |           ✅           |     ✅     |     ❌      |
|      Fragmentation      |        ✅        |         ✅         |           ❌           |     ✅     |     ❌      |

## Automatic unpacking of package fields

Often, when describing endpoints, it is necessary to get fields from the package.

Controller-style

```c#
class ItemsController : IController
{
     [Route("items/delete")]
     [Consumer]
     [Schema(key: nameof(Package.Body), type: typeof(int))]
     public void DeleteItem(Package package)
     {
         var index = package.GetBodyAs<int>();
         ICourier courier = package.Courier!;
         // ...
     }
}
```

Builder style

```c#
builder.Endpoints.AddConsumer(
     new Route("items/delete"),
     package =>
     {
         var index = package.GetBodyAs<int>();
         ICourier courier = package.Courier!;
         // ...
     },
     requestSchemaPatch: new PackageSchema { { nameof(Package.Body), typeof(int) } });
```

There is an automatic unpacking of the package fields.

Controller-style

```c#
class ItemsController : IController
{
     [Route("items/delete")]
     [Consumer]
     [Schema(key: nameof(Package.Body), type: typeof(int))]
     public void DeleteItem([Body] int index, ICourier courier)
     {
         // ...
     }
}
```

Builder style

```c#
builder.Endpoints.AddConsumer(
     new Route("items/delete"),
     ([Body] int index, ICourier courier) =>
     {
         // ...
     },
     requestSchemaPatch: new PackageSchema { { nameof(Package.Body), typeof(int) } });
```

For automatic unpacking of the `Body`, `Error`, `Sender`, `Receiver` fields, you must specify for the parameter
corresponding attribute: `[Body]`, `[Error]`, `[Sender]`, `[Receiver]`. Also, the type of the parameter must match the
type of the field in the package. The remaining fields of the package can be got without specifying attributes. There is
also, a `[Field]` universal attribute that allows you to get a package field by its key.

## Automatic PackageSchemaPatch

If you use automatic unpacking of package fields, then PackageSchemaPatch is generated automatically when
endpoint is registered.

Controller-style

```c#
class ItemsController : IController
{
     [Route("items/delete")]
     [Consumer]
     public void DeleteItem([Body] Guid id, [Error] int code)
     {
         // ...
     }
}
```

Builder style

```c#
builder.Endpoints.AddConsumer(
     new Route("items/delete"),
     ([Body] int index, [Error] int code) =>
     {
         // ...
     });
```

For the `"items/delete"` endpoint, a `RequestSchemaPatch` will be automatically created:

```json
{
  "Body": "Guid",
  "Error": "int"
}
```

ResponseSchemaPatch can also be created automatically.
It is created if the endpoint type provides a response package and the return type of the method is non `Package`.

Controller-style

```c#
class ItemsController : IController
{
     [Route("items/get")]
     [Exchanger]
     public Item GetItem()
     {
         // ...
     }
}
```

Builder style

```c#
builder.Endpoints.AddExchanger(
     new Route("items/get"),
     () => new Item());
```

For endpoint on the route `"items/get"`, a `ResponseSchemaPatch` will be automatically generated:

```json
{
  "Body": "Item"
}
```

Often the PackageSchemaPatch is generated correctly, but you can set it explicitly.
PackageSchemaPatch with the `[Schema]` attribute takes precedence.