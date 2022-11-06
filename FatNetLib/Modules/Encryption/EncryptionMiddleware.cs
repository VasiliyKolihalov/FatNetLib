using System.Security.Cryptography;
using Kolyhalov.FatNetLib.Middlewares;
using Microsoft.Extensions.Logging;

namespace Kolyhalov.FatNetLib.Modules.Encryption;

// Todo: make this class thread-safe
public class EncryptionMiddleware : IMiddleware, IPeerRegistry
{
    private readonly IDictionary<int, byte[]> _keys = new Dictionary<int, byte[]>();
    private readonly IDictionary<int, int> _nonEncodingPeriods = new Dictionary<int, int>();
    private readonly int _maxNonEncodingPeriod;
    private readonly ILogger _logger;

    public EncryptionMiddleware(int maxNonEncodingPeriod, ILogger logger)
    {
        _maxNonEncodingPeriod = maxNonEncodingPeriod;
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
        _nonEncodingPeriods.Remove(peerId);
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
            HandleNonEncodingPeriod(toPeerId);
            return;
        }

        package.Serialized = SymmetricEncrypt(package.Serialized, _keys[toPeerId]);
    }

    private void HandleNonEncodingPeriod(int peerId)
    {
        if (_nonEncodingPeriods.ContainsKey(peerId))
        {
            _nonEncodingPeriods[peerId] -= 1;
        }
        else
        {
            _nonEncodingPeriods[peerId] = _maxNonEncodingPeriod - 1;
        }

        if (_nonEncodingPeriods[peerId] < 0)
            throw new FatNetLibException("Encryption key was not found");

        _logger.LogDebug(
            "Using non-encoding period for encryption, {Periods} periods left", _nonEncodingPeriods[peerId]);
    }

    private static byte[] SymmetricEncrypt(byte[] plainText, byte[] key)
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
