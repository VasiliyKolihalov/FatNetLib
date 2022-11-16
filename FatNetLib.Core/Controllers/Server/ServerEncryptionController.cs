using Kolyhalov.FatNetLib.Core.Attributes;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Services.Server;

namespace Kolyhalov.FatNetLib.Core.Controllers.Server
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
            ICourier courier = handshakePackage.Courier!;
            _service.ExchangePublicKeys(clientPeerId, courier);
            return new Package();
        }
    }
}
