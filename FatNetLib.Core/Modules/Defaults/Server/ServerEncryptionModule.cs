using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Controllers.Server;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Middlewares;
using Kolyhalov.FatNetLib.Core.Services.Server;

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
                .PutScript("RegisterMiddlewares", _ =>
                {
                    _.Get<IList<IMiddleware>>("SendingMiddlewares")
                        .Add(_.Get<EncryptionMiddleware>());
                    _.Get<IList<IMiddleware>>("ReceivingMiddlewares")
                        .Add(_.Get<DecryptionMiddleware>());
                })
                .PutDependency<IServerEncryptionService>(_ => new ServerEncryptionService(
                    _.Get<IEncryptionPeerRegistry>("EncryptionPeerRegistry"),
                    _.Get<IEncryptionPeerRegistry>("DecryptionPeerRegistry")))
                .PutController(_ => new ServerEncryptionController(_.Get<IServerEncryptionService>()));
        }
    }
}
