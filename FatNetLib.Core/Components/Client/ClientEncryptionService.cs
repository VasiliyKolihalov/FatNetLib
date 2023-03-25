using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core.Components.Client
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

        public byte[] ExchangePublicKeys(byte[] serverPublicKey, INetPeer serverPeer)
        {
            var algorithm = new EcdhAlgorithm();
            byte[] sharedSecret = algorithm.CalculateSharedSecret(serverPublicKey);
            _encryptionRegistry.RegisterPeer(serverPeer, sharedSecret);
            _decryptionRegistry.RegisterPeer(serverPeer, sharedSecret);
            return algorithm.MyPublicKey;
        }
    }
}
