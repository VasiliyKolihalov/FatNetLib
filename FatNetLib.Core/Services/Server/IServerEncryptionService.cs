namespace Kolyhalov.FatNetLib.Core.Services.Server
{
    public interface IServerEncryptionService
    {
        public void ExchangePublicKeys(int clientPeerId, ICourier courier);
    }
}
