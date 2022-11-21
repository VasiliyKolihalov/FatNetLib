namespace Kolyhalov.FatNetLib.Core.Services.Client
{
    public class ClientEncryptionService : IClientEncryptionService
    {
        private readonly IEncryptionPeerRegistry _encryptionRegistry;
        private readonly IEncryptionPeerRegistry _decryptionRegistry;

        public ClientEncryptionService(
            IEncryptionPeerRegistry encryptionRegistry,
            IEncryptionPeerRegistry decryptionRegistry)
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
}
