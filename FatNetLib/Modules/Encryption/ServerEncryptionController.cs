using Kolyhalov.FatNetLib.Attributes;

namespace Kolyhalov.FatNetLib.Modules.Encryption
{
    [Route("fat-net-lib/encryption")]
    [Initial]
    public class ServerEncryptionController : IController
    {
        private readonly IServerEncryptionService _service;

        public ServerEncryptionController(IServerEncryptionService service)
        {
            _service = service;
        }

        [Route("public-keys/exchange")]
        public Package ExchangePublicKeys(Package handshakePackage)
        {
            int clientPeerId = handshakePackage.FromPeerId!.Value;
            IClient client = handshakePackage.Client!;
            _service.ExchangePublicKeys(clientPeerId, client);
            return new Package();
        }
    }
}
