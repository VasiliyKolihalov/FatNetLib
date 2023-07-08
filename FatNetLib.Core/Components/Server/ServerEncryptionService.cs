﻿using System.Threading.Tasks;
using Kolyhalov.FatNetLib.Core.Couriers;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core.Components.Server
{
    public class ServerEncryptionService : IServerEncryptionService
    {
        private readonly IEncryptionPeerRegistry _encryptionRegistry;
        private readonly IEncryptionPeerRegistry _decryptionRegistry;

        public ServerEncryptionService(
            IEncryptionPeerRegistry encryptionRegistry,
            IEncryptionPeerRegistry decryptionRegistry)
        {
            _encryptionRegistry = encryptionRegistry;
            _decryptionRegistry = decryptionRegistry;
        }

        public async Task ExchangePublicKeysAsync(INetPeer clientPeer, ICourier courier)
        {
            var algorithm = new EcdhAlgorithm();
            var serverPublicKeyPackage = new Package
            {
                Route = new Route("/fat-net-lib/encryption/public-keys/exchange"),
                Body = algorithm.MyPublicKey,
                Receiver = clientPeer
            };
            serverPublicKeyPackage.SetNonSendingField("SkipEncryption", value: true);
            Package clientPublicKeyPackage = (await courier.SendAsync(serverPublicKeyPackage))!;

            byte[] clientPublicKey = clientPublicKeyPackage.GetBodyAs<byte[]>();
            byte[] sharedSecret = algorithm.CalculateSharedSecret(clientPublicKey);
            _encryptionRegistry.RegisterPeer(clientPeer, sharedSecret);
            _decryptionRegistry.RegisterPeer(clientPeer, sharedSecret);
        }
    }
}
