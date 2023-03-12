# Courier

The courier is the application component that is responsible for sending packages and receiving responses. There are 2
types of couriers: for the server - `ServerCourier` and for the client `ClientCourier`.

## Access to courier

The first way is to get the courier after build the application. For this you need to use`FatNetLib` properties:

```c#
ICourier courier = builder.BuildAndRun().Courier;
```

```c#
IServerCourier courier = builder.BuildAndRun().ServerCourier!;
// or
IClientCourier courier = builder.BuildAndRun().ClientCourier!;
```

The second way is to get the courier from the received package in endpoint. For this, you need to use properties
of `Package` or use auto-unpacking of package fields:

```c#
[Route("add")]
[Consumer]
public void AddItem(ICourier courier)
{
     // ...
}
```

```c#
[Route("add")]
[Consumer]
public void AddItem(Package package)
{
     IServerCourier courier = package.ServerCourier!;
     // or
     IClientCourier courier = package.ClientCourier!;
}
```

Learn more about [Automatic unpacking of package fields](2-endpoints.md).

Please note that there is only one courier instance per application, and the `ServerCourier` and `ClientCourier`
properties are simply cast value that stored in the `Courier` property.

## Working with a courier

### Sending to specific peer
To send a package over the network to a specific peer, you need to use the`SendAsync()` method, which available from all
types of couriers. It accepts a package that needs  `Route` and  `Receiver` to be specified.

```c#
[Route("ping")]
[Consumer(Reliability.Sequenced)]
public async Task PingAsync([Sender] INetPeer senderPeer, ICourier courier)
{
     await courier.SendAsync(new Package
     {
         Route = new Route("pong"),
         Receiver = senderPeer
     });
}
```

If the type of endpoint for which the send package is supports the response package, then it can be received in the
following way:

```c#
[Route("ping")]
[Consumer(Reliability.Sequenced)]
public async Task PingAsync([Sender] INetPeer senderPeer, ICourier courier)
{
     Package response = (await courier.SendAsync(new Package
     {
         Route = new Route("pong"),
         Receiver = senderPeer
     }))!;
     // ...
}
```

Waiting for a response package has a timeout - `ExchangeTimeout`. It is set from the configuration. If the wait exceeds
this timeout, then `FatNetLib` will throw `FatNetLibException`.

### Emitting event
To emit an event, you need to use the `EmitEvent()` method, which available from all types of couriers. It accepts a
package that needs `Route` to be specified. Only *EventListener* can be emitted.

```c#
await courier.EmitEventAsync(new Package
{
     Route = new Route("my-event-listener")
});
```

### Sending to all clients
To send a package to all clients peers, you need to use the `BroadcastAsync()` method. It's available only from `ServerCourier`.

```c#
IServerCourier courier = builder.BuildAndRun().ServerCourier!;
await courier.BroadcastAsync(new Package
{
     new Route("ping")
});
```

If you need, you can ignore a certain peer

```c#
[Route("add")]
[Consumer]
public Task AddItemAsync([Sender] INetPeer sender)
{
    // ...
    IServerCourier courier = package.ServerCourier!;
    await courier.BroadcastAsync(new Package
    {
        new Route("update")
    }, ignorePeer: sender);

}
```

### Sending to server
To send a package to the server peer, use the `SendToServerAsync()` method. It's available
only from `ClientCourier`.

```c#
IClientCourier courier = builder.BuildAndRun().ClientCourier!;
await courier.SendToServer(new Package
{
     new Route("ping")
});
```