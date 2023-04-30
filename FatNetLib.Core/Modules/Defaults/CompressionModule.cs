using System.IO.Compression;
using Kolyhalov.FatNetLib.Core.Components;
using Kolyhalov.FatNetLib.Core.Middlewares;

namespace Kolyhalov.FatNetLib.Core.Modules.Defaults
{
    public class CompressionModule : IModule
    {
        private readonly CompressionLevel _compressionLevel;

        public CompressionModule(CompressionLevel compressionLevel)
        {
            _compressionLevel = compressionLevel;
        }

        public void Setup(IModuleContext moduleContext)
        {
            moduleContext
                .PutDependency<ICompressionAlgorithm>(_ => new GZipAlgorithm(_compressionLevel))
                .PutSendingMiddleware(_ => new CompressionMiddleware(_.Get<ICompressionAlgorithm>()))
                .PutReceivingMiddleware(_ => new DecompressionMiddleware(_.Get<ICompressionAlgorithm>()));
        }
    }
}
