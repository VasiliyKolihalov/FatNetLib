namespace Kolyhalov.FatNetLib.Modules.Encryption
{
    public interface IServerEncryptionService
    {
        public void ExchangePublicKeys(int clientPeerId, IClient client);
    }
}
