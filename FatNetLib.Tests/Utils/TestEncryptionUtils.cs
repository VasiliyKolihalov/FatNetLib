using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Kolyhalov.FatNetLib.Tests.Utils;

public static class TestEncryptionUtils
{
    public static class Aes
    {
        public static byte[] Encrypt(byte[] plainText, byte[] key)
        {
            using var aes = System.Security.Cryptography.Aes.Create();
            aes.Key = key;
            aes.GenerateIV();
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            return aes.IV.Concat(PerformCryptography(encryptor, plainText)).ToArray();
        }

        public static byte[] Decrypt(byte[] encryptedPackage, byte[] key)
        {
            using var aes = System.Security.Cryptography.Aes.Create();
            aes.Key = key;
            aes.IV = encryptedPackage.Take(aes.IV.Length).ToArray();
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            byte[] cipherText = encryptedPackage.Skip(aes.IV.Length)
                .Take(encryptedPackage.Length - aes.IV.Length)
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
