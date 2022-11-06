using Kolyhalov.FatNetLib.Microtypes;

namespace Kolyhalov.FatNetLib.Modules.Encryption;

public class ServerEncryptionService : IServerEncryptionService
{
    private readonly IPeerRegistry _encryptionRegistry;
    private readonly IPeerRegistry _decryptionRegistry;

    public ServerEncryptionService(IPeerRegistry encryptionRegistry, IPeerRegistry decryptionRegistry)
    {
        _encryptionRegistry = encryptionRegistry;
        _decryptionRegistry = decryptionRegistry;
    }

    public void ExchangePublicKeys(int clientPeerId, IClient client)
    {
        var algorithm = new EcdhAlgorithm();
        var serverPublicKeyPackage = new Package
        {
            Route = new Route("/fat-net-lib/encryption/public-keys/exchange"),
            Body = algorithm.MyPublicKey,
            ToPeerId = clientPeerId
        };
        serverPublicKeyPackage.SetNonSendingField("SkipEncryption", value: true);
        Package clientPublicKeyPackage = client.SendPackage(serverPublicKeyPackage)!;

        byte[] clientPublicKey = clientPublicKeyPackage.GetBodyAs<byte[]>()!;
        byte[] sharedSecret = algorithm.CalculateSharedSecret(clientPublicKey);
        _encryptionRegistry.RegisterPeer(clientPeerId, sharedSecret);
        _decryptionRegistry.RegisterPeer(clientPeerId, sharedSecret);
    }
}
