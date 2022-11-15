using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Controllers.Server;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Middlewares;
using Kolyhalov.FatNetLib.Core.Services.Server;

namespace Kolyhalov.FatNetLib.Core.Modules.Defaults.Server
{
    public class ServerEncryptionModule : IModule
    {
        public void Setup(ModuleContext moduleContext)
        {
            var logger = moduleContext.DependencyContext.Get<ILogger>();
            var encryptionMiddleware = new EncryptionMiddleware(maxNonEncryptionPeriod: 1, logger);
            var decryptionMiddleware = new DecryptionMiddleware(maxNonDecryptionPeriod: 3, logger);
            moduleContext.SendingMiddlewares.Add(encryptionMiddleware);
            moduleContext.ReceivingMiddlewares.Insert(0, decryptionMiddleware);
            moduleContext.EndpointRecorder.AddController(
                new ServerEncryptionController(new ServerEncryptionService(
                    encryptionMiddleware,
                    decryptionMiddleware)));
        }

        public IList<IModule>? ChildModules => null;
    }
}
