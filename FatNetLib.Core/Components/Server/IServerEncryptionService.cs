using System.Threading.Tasks;
using Kolyhalov.FatNetLib.Core.Couriers;
using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core.Components.Server
{
    public interface IServerEncryptionService
    {
        public Task ExchangePublicKeysAsync(INetPeer clientPeer, ICourier courier);
    }
}
