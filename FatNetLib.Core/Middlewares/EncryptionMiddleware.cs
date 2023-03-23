using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Kolyhalov.FatNetLib.Core.Components;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Wrappers;

namespace Kolyhalov.FatNetLib.Core.Middlewares
{
    // Todo: make this class thread-safe
    public class EncryptionMiddleware : IMiddleware, IEncryptionPeerRegistry
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

        public void RegisterPeer(INetPeer peer, byte[] key)
        {
            _keys[peer.Id] = key;
        }

        // Todo: call this method after fluent modules implementation in ticket #117
        public void UnregisterPeer(INetPeer peer)
        {
            _keys.Remove(peer.Id);
            _nonEncryptionPeriods.Remove(peer.Id);
        }

        public void Process(Package package)
        {
            if (package.Serialized is null)
                throw new FatNetLibException($"Package must contain {nameof(package.Serialized)} field");
            if (package.ToPeer is null)
                throw new FatNetLibException($"Package must contain {nameof(package.ToPeer)} field");

            if (package.NonSendingFields.ContainsKey("SkipEncryption")
                && package.GetNonSendingField<bool>("SkipEncryption"))
                return;

            INetPeer toPeer = package.ToPeer!;
            if (!_keys.ContainsKey(toPeer.Id))
            {
                HandleNonEncryptionPeriod(toPeer);
                return;
            }

            package.Serialized = Encrypt(package.Serialized!, _keys[toPeer.Id]);
        }

        private void HandleNonEncryptionPeriod(INetPeer peer)
        {
            if (_nonEncryptionPeriods.ContainsKey(peer.Id))
            {
                _nonEncryptionPeriods[peer.Id] -= 1;
            }
            else
            {
                _nonEncryptionPeriods[peer.Id] = _maxNonEncryptionPeriod - 1;
            }

            if (_nonEncryptionPeriods[peer.Id] < 0)
                throw new FatNetLibException("Encryption key was not found");

            _logger.Debug(() =>
                $"Using non-encryption period for encryption, {_nonEncryptionPeriods[peer.Id]} periods left");
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
