namespace Kolyhalov.FatNetLib.Modules.Encryption;

public class ClientEncryptionService : IClientEncryptionService
{
    private readonly IPeerRegistry _encryptionRegistry;
    private readonly IPeerRegistry _decryptionRegistry;

    public ClientEncryptionService(IPeerRegistry encryptionRegistry, IPeerRegistry decryptionRegistry)
    {
        _encryptionRegistry = encryptionRegistry;
        _decryptionRegistry = decryptionRegistry;
    }

    public byte[] ExchangePublicKeys(byte[] serverPublicKey, int serverPeerId)
    {
        var algorithm = new EcdhAlgorithm();
        byte[] sharedSecret = algorithm.CalculateSharedSecret(serverPublicKey);
        _encryptionRegistry.RegisterPeer(serverPeerId, sharedSecret);
        _decryptionRegistry.RegisterPeer(serverPeerId, sharedSecret);
        return algorithm.MyPublicKey;
    }
}
