﻿using Kolyhalov.FatNetLib.Core.Components;
using Kolyhalov.FatNetLib.Core.Components.Client;
using Kolyhalov.FatNetLib.Core.Controllers.Client;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Middlewares;

namespace Kolyhalov.FatNetLib.Core.Modules.Defaults.Client
{
    public class ClientEncryptionModule : IModule
    {
        public void Setup(IModuleContext moduleContext)
        {
            moduleContext
                .PutDependency(_ => new EncryptionMiddleware(maxNonEncryptionPeriod: 2, _.Get<ILogger>()))
                .PutDependency("EncryptionPeerRegistry", _ => _.Get<EncryptionMiddleware>())
                .PutDependency(_ => new DecryptionMiddleware(maxNonDecryptionPeriod: 2, _.Get<ILogger>()))
                .PutDependency("DecryptionPeerRegistry", _ => _.Get<DecryptionMiddleware>())
                .PutSendingMiddleware(_ => _.Get<EncryptionMiddleware>())
                .PutReceivingMiddleware(_ => _.Get<DecryptionMiddleware>())
                .PutDependency<IClientEncryptionService>(_ => new ClientEncryptionService(
                    _.Get<IEncryptionPeerRegistry>("EncryptionPeerRegistry"),
                    _.Get<IEncryptionPeerRegistry>("DecryptionPeerRegistry")))
                .PutController(_ => new ClientEncryptionController(_.Get<IClientEncryptionService>()));
        }
    }
}
