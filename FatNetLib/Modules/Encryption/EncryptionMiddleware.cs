using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Kolyhalov.FatNetLib.Loggers;
using Kolyhalov.FatNetLib.Middlewares;

namespace Kolyhalov.FatNetLib.Modules.Encryption
{
    // Todo: make this class thread-safe
    public class EncryptionMiddleware : IMiddleware, IPeerRegistry
    {
        private readonly IDictionary<int, byte[]> _keys = new Dictionary<int, byte[]>();
        private readonly IDictionary<int, int> _nonEncryptionPeriods = new Dictionary<int, int>();
        private readonly int _maxNonEncryptionPeriod;
        private readonly ILogger _logger;

        public EncryptionMiddleware(int maxNonEncryptionPeriod, ILogger logger)
        {
            _maxNonEncryptionPeriod = maxNonEncryptionPeriod;
            _logger = logger;
        }

        public void RegisterPeer(int peerId, byte[] key)
        {
            _keys[peerId] = key;
        }

        // Todo: call this method after fluent modules implementation in ticket #117
        public void UnregisterPeer(int peerId)
        {
            _keys.Remove(peerId);
            _nonEncryptionPeriods.Remove(peerId);
        }

        public void Process(Package package)
        {
            if (package.GetNonSendingField<bool>("SkipEncryption"))
                return;
            if (package.ToPeerId is null)
                throw new FatNetLibException($"{nameof(package.ToPeerId)} field is missing");
            if (package.Serialized is null)
                throw new FatNetLibException($"{nameof(package.Serialized)} field is missing");

            int toPeerId = package.ToPeerId.Value;
            if (!_keys.ContainsKey(toPeerId))
            {
                HandleNonEncryptionPeriod(toPeerId);
                return;
            }

            package.Serialized = Encrypt(package.Serialized, _keys[toPeerId]);
        }

        private void HandleNonEncryptionPeriod(int peerId)
        {
            if (_nonEncryptionPeriods.ContainsKey(peerId))
            {
                _nonEncryptionPeriods[peerId] -= 1;
            }
            else
            {
                _nonEncryptionPeriods[peerId] = _maxNonEncryptionPeriod - 1;
            }

            if (_nonEncryptionPeriods[peerId] < 0)
                throw new FatNetLibException("Encryption key was not found");

            _logger.Debug(() =>
                $"Using non-encryption period for encryption, {_nonEncryptionPeriods[peerId]} periods left");
        }

        private static byte[] Encrypt(byte[] plainText, byte[] key)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV();
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            return aes.IV.Concat(PerformCryptography(encryptor, plainText)).ToArray();
        }

        private static IEnumerable<byte> PerformCryptography(ICryptoTransform cryptoTransform, byte[] data)
        {
            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write);
            cryptoStream.Write(data, offset: 0, count: data.Length);
            cryptoStream.FlushFinalBlock();
            return memoryStream.ToArray();
        }
    }
}
