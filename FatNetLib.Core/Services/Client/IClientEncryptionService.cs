namespace Kolyhalov.FatNetLib.Core.Services.Client
{
    public interface IClientEncryptionService
    {
        public byte[] ExchangePublicKeys(byte[] serverPublicKey, int serverPeerId);
    }
}
