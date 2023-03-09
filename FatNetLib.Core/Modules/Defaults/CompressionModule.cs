using System.Collections.Generic;
using System.IO.Compression;
using Kolyhalov.FatNetLib.Core.Middlewares;
using Kolyhalov.FatNetLib.Core.Services;

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
                .PutDependency(_ => new CompressionMiddleware(_.Get<ICompressionAlgorithm>()))
                .PutDependency(_ => new DecompressionMiddleware(_.Get<ICompressionAlgorithm>()))
                .PutScript("PutCompressionMiddlewares", _ =>
                {
                    _.Get<IList<IMiddleware>>("SendingMiddlewares")
                        .Add(_.Get<CompressionMiddleware>());
                    _.Get<IList<IMiddleware>>("ReceivingMiddlewares")
                        .Add(_.Get<DecompressionMiddleware>());
                });
        }
    }
}
