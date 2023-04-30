using Kolyhalov.FatNetLib.Core.Components;
using Kolyhalov.FatNetLib.Core.Components.Server;
using Kolyhalov.FatNetLib.Core.Controllers.Server;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Middlewares;

namespace Kolyhalov.FatNetLib.Core.Modules.Defaults.Server
{
    public class ServerEncryptionModule : IModule
    {
        public void Setup(IModuleContext moduleContext)
        {
            moduleContext
                .PutDependency(_ => new EncryptionMiddleware(maxNonEncryptionPeriod: 1, _.Get<ILogger>()))
                .PutDependency("EncryptionPeerRegistry", _ => _.Get<EncryptionMiddleware>())
                .PutDependency(_ => new DecryptionMiddleware(maxNonDecryptionPeriod: 3, _.Get<ILogger>()))
                .PutDependency("DecryptionPeerRegistry", _ => _.Get<DecryptionMiddleware>())
                .PutSendingMiddleware(_ => _.Get<EncryptionMiddleware>())
                .PutReceivingMiddleware(_ => _.Get<DecryptionMiddleware>())
                .PutDependency<IServerEncryptionService>(_ => new ServerEncryptionService(
                    _.Get<IEncryptionPeerRegistry>("EncryptionPeerRegistry"),
                    _.Get<IEncryptionPeerRegistry>("DecryptionPeerRegistry")))
                .PutController(_ => new ServerEncryptionController(_.Get<IServerEncryptionService>()));
        }
    }
}
