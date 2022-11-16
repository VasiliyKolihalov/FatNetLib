using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Models;

namespace Kolyhalov.FatNetLib.Core.Services.Server
{
    public class ServerEncryptionService : IServerEncryptionService
    {
        private readonly IPeerRegistry _encryptionRegistry;
        private readonly IPeerRegistry _decryptionRegistry;

        public ServerEncryptionService(IPeerRegistry encryptionRegistry, IPeerRegistry decryptionRegistry)
        {
            _encryptionRegistry = encryptionRegistry;
            _decryptionRegistry = decryptionRegistry;
        }

        public void ExchangePublicKeys(int clientPeerId, ICourier courier)
        {
            var algorithm = new EcdhAlgorithm();
            var serverPublicKeyPackage = new Package
            {
                Route = new Route("/fat-net-lib/encryption/public-keys/exchange"),
                Body = algorithm.MyPublicKey,
                ToPeerId = clientPeerId
            };
            serverPublicKeyPackage.SetNonSendingField("SkipEncryption", value: true);
            Package clientPublicKeyPackage = courier.SendPackage(serverPublicKeyPackage)!;

            byte[] clientPublicKey = clientPublicKeyPackage.GetBodyAs<byte[]>();
            byte[] sharedSecret = algorithm.CalculateSharedSecret(clientPublicKey);
            _encryptionRegistry.RegisterPeer(clientPeerId, sharedSecret);
            _decryptionRegistry.RegisterPeer(clientPeerId, sharedSecret);
        }
    }
}
