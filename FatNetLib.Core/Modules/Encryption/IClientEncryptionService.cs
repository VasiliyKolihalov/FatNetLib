﻿namespace Kolyhalov.FatNetLib.Core.Modules.Encryption
{
    public interface IClientEncryptionService
    {
        public byte[] ExchangePublicKeys(byte[] serverPublicKey, int serverPeerId);
    }
}