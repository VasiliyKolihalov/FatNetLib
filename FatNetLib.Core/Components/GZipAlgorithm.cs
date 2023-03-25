using System.IO;
using System.IO.Compression;

namespace Kolyhalov.FatNetLib.Core.Components
{
    public class GZipAlgorithm : ICompressionAlgorithm
    {
        private readonly CompressionLevel _compressionLevel;

        public GZipAlgorithm(CompressionLevel compressionLevel)
        {
            _compressionLevel = compressionLevel;
        }

        public byte[] Compress(byte[] data)
        {
            using var compressedStream = new MemoryStream();
            using var zipStream = new GZipStream(compressedStream, _compressionLevel);
            zipStream.Write(data, offset: 0, count: data.Length);
            zipStream.Close();
            return compressedStream.ToArray();
        }

        public byte[] Decompress(byte[] data)
        {
            using var compressedStream = new MemoryStream(data);
            using var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
            using var decompressedStream = new MemoryStream();
            zipStream.CopyTo(decompressedStream);
            return decompressedStream.ToArray();
        }
    }
}
