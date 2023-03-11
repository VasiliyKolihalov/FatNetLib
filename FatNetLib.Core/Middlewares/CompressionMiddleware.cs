using Kolyhalov.FatNetLib.Core.Models;
using Kolyhalov.FatNetLib.Core.Services;

namespace Kolyhalov.FatNetLib.Core.Middlewares
{
    public class CompressionMiddleware : IMiddleware
    {
        private readonly ICompressionAlgorithm _compressionAlgorithm;

        public CompressionMiddleware(ICompressionAlgorithm compressionAlgorithm)
        {
            _compressionAlgorithm = compressionAlgorithm;
        }

        public void Process(Package package)
        {
            package.Serialized = _compressionAlgorithm.Compress(package.Serialized!);
        }
    }
}
