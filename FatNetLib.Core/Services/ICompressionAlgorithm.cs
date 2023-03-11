namespace Kolyhalov.FatNetLib.Core.Services
{
    public interface ICompressionAlgorithm
    {
        public byte[] Compress(byte[] data);

        public byte[] Decompress(byte[] data);
    }
}
