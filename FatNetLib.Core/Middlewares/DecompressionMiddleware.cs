using Kolyhalov.FatNetLib.Core.Components;
using Kolyhalov.FatNetLib.Core.Models;

namespace Kolyhalov.FatNetLib.Core.Middlewares
{
    public class DecompressionMiddleware : IMiddleware
    {
        private readonly ICompressionAlgorithm _compressionAlgorithm;

        public DecompressionMiddleware(ICompressionAlgorithm compressionAlgorithm)
        {
            _compressionAlgorithm = compressionAlgorithm;
        }

        public void Process(Package package)
        {
            package.Serialized = _compressionAlgorithm.Decompress(package.Serialized!);
        }
    }
}
