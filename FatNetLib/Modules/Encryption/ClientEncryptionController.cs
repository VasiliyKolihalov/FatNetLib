using Kolyhalov.FatNetLib.Attributes;

namespace Kolyhalov.FatNetLib.Modules.Encryption;

[Route("fat-net-lib/encryption")]
[Initial]
public class ClientEncryptionController : IController
{
    private readonly IPeerRegistry _encryptionMiddleware;
    private readonly IPeerRegistry _decryptionMiddleware;

    public ClientEncryptionController(
        IPeerRegistry encryptionMiddleware,
        IPeerRegistry decryptionMiddleware)
    {
        _encryptionMiddleware = encryptionMiddleware;
        _decryptionMiddleware = decryptionMiddleware;
    }

    [Route("public-keys/exchange")]
    [Schema(key: nameof(Package.Body), type: typeof(byte[]))]
    [return: Schema(key: nameof(Package.Body), type: typeof(byte[]))]
    public Package ExchangePublicKeys(Package serverPublicKeyPackage)
    {
        var algorithm = new EcdhAlgorithm();
        int serverPeerId = serverPublicKeyPackage.FromPeerId!.Value;
        byte[] serverPublicKey = serverPublicKeyPackage.GetBodyAs<byte[]>()!;
        byte[] sharedSecret = algorithm.CalculateSharedSecret(serverPublicKey);
        _encryptionMiddleware.RegisterPeer(serverPeerId, sharedSecret);
        _decryptionMiddleware.RegisterPeer(serverPeerId, sharedSecret);
        var clientPublicKeyPackage = new Package { Body = algorithm.MyPublicKey };
        clientPublicKeyPackage.SetNonSendingField("SkipEncryption", true);
        return clientPublicKeyPackage;
    }
}
