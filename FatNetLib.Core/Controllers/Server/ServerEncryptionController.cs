using Kolyhalov.FatNetLib.Core.Attributes;
using Kolyhalov.FatNetLib.Core.Couriers;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Services.Server;
using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core.Controllers.Server
{
    [Route("fat-net-lib/encryption")]
    public class ServerEncryptionController : IController
    {
        private readonly IServerEncryptionService _service;

        public ServerEncryptionController(IServerEncryptionService service)
        {
            _service = service;
        }

        [Initializer]
        [Route("public-keys/exchange")]
        public Package ExchangePublicKeys(Package handshakePackage)
        {
            INetPeer clientPeer = handshakePackage.FromPeer!;
            ICourier courier = handshakePackage.Courier!;
            _service.ExchangePublicKeys(clientPeer, courier);
            return new Package();
        }
    }
}
