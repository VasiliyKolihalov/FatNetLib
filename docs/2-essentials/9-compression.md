# Compression

When sending a packet over the network, it is possible to save time on its delivery by reducing its size using
compression. 
In addition to reducing the delivery time, the costs and time for preparing the package increase.
FatNteLib provides a standard compression module - `CompressionModule`. 
This module uses the GZip algorithm and has the ability to adjust the compression level.

## Working with `CompressionModule`

The `CompressionModule` module must be registered on both the client and the server. Upon registration, we have the
ability to select the compression level from the available ones:

* NoCompression — no compression should be performed on the package.
* Fastest — compression operation should complete as quickly as possible, even if the resulting package is not optimally
  compressed.
* Optimal — compression operation should optimally balance compression speed and output package. The Default level.
* SmallestSize — compression operation should create output as small as possible, even if the operation takes a longer
  time to complete.

The module itself does not affect other modules and can be registered in any order, but it adds two middlewares, the
order of which is important.
You should remember that compression middleware works with serialized packets. 
Consider an example using `CompressionModule` together with `JsonModule`.

```c#
var builder = new FatNetLibBuilder
{
     Modules =
     {
         // ...
         new JsonModule(),
         new CompressionModule(CompressionLevel.Optimal)
     },
     SendingMiddlewaresOrder = new[]
     {
         typeof(JsonSerializationMiddleware),
         // ...
         typeof(CompressionMiddleware)
     },
     ReceivingMiddlewaresOrder = new[]
     {
         typeof(DecompressionMiddleware),
         // ...
         typeof(JsonDeserializationMiddleware),
     }
};
// ...
```

In this example, we see the following order of SendingMiddlewares: first, the packet is serialized and then compressed.
And for ReceivingMiddlewaresOrder: the package is first decompressed and then deserialized. This is due to the fact that
compression works on a serialized packet, an array of bytes.