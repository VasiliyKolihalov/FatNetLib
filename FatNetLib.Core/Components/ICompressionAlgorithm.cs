namespace Kolyhalov.FatNetLib.Core.Components
{
    public interface ICompressionAlgorithm
    {
        public byte[] Compress(byte[] data);

        public byte[] Decompress(byte[] data);
    }
}
