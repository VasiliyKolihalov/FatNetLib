using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core.Components.Client
{
    public interface IClientEncryptionService
    {
        public byte[] ExchangePublicKeys(byte[] serverPublicKey, INetPeer serverPeer);
    }
}
