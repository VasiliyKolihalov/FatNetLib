using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Kolyhalov.FatNetLib.Loggers;
using Kolyhalov.FatNetLib.Middlewares;

namespace Kolyhalov.FatNetLib.Modules.Encryption
{
// Todo: make this class thread-safe
    public class DecryptionMiddleware : IMiddleware, IPeerRegistry
    {
        private readonly IDictionary<int, byte[]> _keys = new Dictionary<int, byte[]>();
        private readonly IDictionary<int, int> _nonDecryptionPeriods = new Dictionary<int, int>();
        private readonly int _maxNonDecryptionPeriod;
        private readonly ILogger _logger;

        public DecryptionMiddleware(int maxNonDecryptionPeriod, ILogger logger)
        {
            _maxNonDecryptionPeriod = maxNonDecryptionPeriod;
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
        }

        public void Process(Package package)
        {
            if (package.FromPeerId is null)
                throw new FatNetLibException($"{nameof(package.FromPeerId)} field is missing");
            if (package.Serialized is null)
                throw new FatNetLibException($"{nameof(package.Serialized)} field is missing");

            int fromPeerId = package.FromPeerId.Value;
            if (!_keys.ContainsKey(fromPeerId))
            {
                HandleNonDecryptionPeriod(fromPeerId);
                return;
            }

            package.Serialized = Decrypt(package.Serialized, _keys[fromPeerId]);
        }

        private void HandleNonDecryptionPeriod(int peerId)
        {
            if (_nonDecryptionPeriods.ContainsKey(peerId))
            {
                _nonDecryptionPeriods[peerId] -= 1;
            }
            else
            {
                _nonDecryptionPeriods[peerId] = _maxNonDecryptionPeriod - 1;
            }

            if (_nonDecryptionPeriods[peerId] < 0)
                throw new FatNetLibException("Decryption key was not found");

            _logger.Debug(
                "Using non-decryption period for decryption, {Periods} periods left", _nonDecryptionPeriods[peerId]);
        }

        private static byte[] Decrypt(IReadOnlyCollection<byte> encryptedPackage, byte[] key)
        {
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = encryptedPackage.Take(aes.IV.Length).ToArray();
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            byte[] cipherText = encryptedPackage.Skip(aes.IV.Length)
                .Take(encryptedPackage.Count - aes.IV.Length)
                .ToArray();
            return PerformCryptography(decryptor, cipherText);
        }

        private static byte[] PerformCryptography(ICryptoTransform cryptoTransform, byte[] data)
        {
            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write);
            cryptoStream.Write(data, offset: 0, count: data.Length);
            cryptoStream.FlushFinalBlock();
            return memoryStream.ToArray();
        }
    }
}
