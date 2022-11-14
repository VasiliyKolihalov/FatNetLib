using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Controllers.Client;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Middlewares;
using Kolyhalov.FatNetLib.Core.Services.Client;

namespace Kolyhalov.FatNetLib.Core.Modules.Defaults.Client
{
    public class ClientEncryptionModule : IModule
    {
        public void Setup(ModuleContext moduleContext)
        {
            var logger = moduleContext.DependencyContext.Get<ILogger>();
            var encryptionMiddleware = new EncryptionMiddleware(maxNonEncryptionPeriod: 2, logger);
            var decryptionMiddleware = new DecryptionMiddleware(maxNonDecryptionPeriod: 2, logger);
            moduleContext.SendingMiddlewares.Add(encryptionMiddleware);
            moduleContext.ReceivingMiddlewares.Insert(0, decryptionMiddleware);
            moduleContext.EndpointRecorder.AddController(
                new ClientEncryptionController(new ClientEncryptionService(
                    encryptionMiddleware,
                    decryptionMiddleware)));
        }

        public IList<IModule>? ChildModules => null;
    }
}
