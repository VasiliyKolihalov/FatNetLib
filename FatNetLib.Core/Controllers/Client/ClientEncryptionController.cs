using Kolyhalov.FatNetLib.Core.Attributes;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Services.Client;
using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core.Controllers.Client
{
    [Route("fat-net-lib/encryption")]
    public class ClientEncryptionController : IController
    {
        private readonly IClientEncryptionService _service;

        public ClientEncryptionController(IClientEncryptionService service)
        {
            _service = service;
        }

        [Initializer]
        [Route("public-keys/exchange")]
        [Schema(key: nameof(Package.Body), type: typeof(byte[]))]
        [return: Schema(key: nameof(Package.Body), type: typeof(byte[]))]
        public Package ExchangePublicKeys(Package serverPublicKeyPackage)
        {
            byte[] serverPublicKey = serverPublicKeyPackage.GetBodyAs<byte[]>();
            INetPeer serverPeer = serverPublicKeyPackage.FromPeer!;
            byte[] clientPublicKey = _service.ExchangePublicKeys(serverPublicKey, serverPeer);

            var clientPublicKeyPackage = new Package { Body = clientPublicKey };
            clientPublicKeyPackage.SetNonSendingField("SkipEncryption", value: true);
            return clientPublicKeyPackage;
        }
    }
}
