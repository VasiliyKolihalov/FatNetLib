using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core.Services.Client
{
    public interface IClientEncryptionService
    {
        public byte[] ExchangePublicKeys(byte[] serverPublicKey, INetPeer serverPeer);
    }
}
