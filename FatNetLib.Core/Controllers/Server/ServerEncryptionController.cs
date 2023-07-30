using System.Threading.Tasks;
using Kolyhalov.FatNetLib.Core.Attributes;
using Kolyhalov.FatNetLib.Core.Components.Server;
using Kolyhalov.FatNetLib.Core.Couriers;
using Kolyhalov.FatNetLib.Core.Models;
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
        public async Task<Package> ExchangePublicKeysAsync([Sender] INetPeer clientPeer, ICourier courier)
        {
            await _service.ExchangePublicKeysAsync(clientPeer, courier);
            return new Package();
        }
    }
}
