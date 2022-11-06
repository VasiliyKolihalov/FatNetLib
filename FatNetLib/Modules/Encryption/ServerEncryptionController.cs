using Kolyhalov.FatNetLib.Attributes;
using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.Middlewares;

namespace Kolyhalov.FatNetLib.Modules.Encryption;

[Route("fat-net-lib/encryption")]
[Initial]
public class ServerEncryptionController : IController
{
    private readonly IPeerRegistry _encryptionMiddleware;
    private readonly IPeerRegistry _decryptionMiddleware;

    public ServerEncryptionController(
        IPeerRegistry encryptionMiddleware,
        IPeerRegistry decryptionMiddleware)
    {
        _encryptionMiddleware = encryptionMiddleware;
        _decryptionMiddleware = decryptionMiddleware;
    }

    [Route("public-keys/exchange")]
    public Package ExchangePublicKeys(Package handshakePackage)
    {
        var algorithm = new EcdhAlgorithm();
        int clientPeerId = handshakePackage.FromPeerId!.Value;
        var serverPublicKeyPackage = new Package
        {
            Route = new Route("/fat-net-lib/encryption/public-keys/exchange"),
            Body = algorithm.MyPublicKey,
            ToPeerId = clientPeerId
        };
        serverPublicKeyPackage.SetNonSendingField("SkipEncryption", false);
        Package clientPublicKeyPackage = handshakePackage.Client!.SendPackage(serverPublicKeyPackage)!;

        byte[] clientPublicKey = clientPublicKeyPackage.GetBodyAs<byte[]>()!;
        byte[] sharedSecret = algorithm.CalculateSharedSecret(clientPublicKey);
        _encryptionMiddleware.RegisterPeer(clientPeerId, sharedSecret);
        _decryptionMiddleware.RegisterPeer(clientPeerId, sharedSecret);
        return new Package();
    }
}
