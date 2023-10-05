# Encryption

To guarantee the security of packages sent over the network, `FatNetLib` has a built-in encryption mechanism. It is added
by `ServerEncryptionModule` and `ClientEncryptionModule`, which are built into the standard
modules `DefaultServerModule` and `DefaultClientModule`. Encryption modules implement the AES standard

Encryption in `FatNetLib` consists of two stages:

1. The server and client exchange public keys using `Initializer` endpoints. During this exchange, packages are not
   amenable to encryption by the `NonDecryptionPeriod` for new connected peers.

2. Then the encryption works as normal, and all outgoing and incoming packages are encrypted and decrypted.

## Working with `ServerEncryptionModule` and `ClientEncryptionModule`

Encryption modules add two middleware encryption modules, the order of which needs to be configured. It is important to
remember that encryption middleware works with serialized packages. Consider an example. Used together with `JsonModule`.

```c#

var builder = new FatNetLibBuilder
{
     Modules =
     {
         new DefaultServerModule(),
         new JsonModule(),
     },
     SendingMiddlewaresOrder = new[]
     {
         typeof(JsonSerializationMiddleware),
         typeof(EncryptionMiddleware)
     },
     ReceivingMiddlewaresOrder = new[]
     {
         typeof(DecryptionMiddleware),
         typeof(JsonDeserializationMiddleware),
     }
};
```

After setting the order execution of middleware, the package will first be decrypted and then deserialized.
And the outgoing package will be serialized and then encrypted.