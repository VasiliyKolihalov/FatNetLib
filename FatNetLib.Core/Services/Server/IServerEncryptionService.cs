using Kolyhalov.FatNetLib.Core.Couriers;
using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core.Services.Server
{
    public interface IServerEncryptionService
    {
        public void ExchangePublicKeys(INetPeer clientPeer, ICourier courier);
    }
}
