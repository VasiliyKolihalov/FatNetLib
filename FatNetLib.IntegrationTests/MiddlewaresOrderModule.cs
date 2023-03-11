using System.Collections.Generic;
using System.Linq;
using Kolyhalov.FatNetLib.Core.Middlewares;
using Kolyhalov.FatNetLib.Core.Modules;
using NUnit.Framework;
using IMicrosoftLogger = Microsoft.Extensions.Logging.ILogger;

namespace Kolyhalov.FatNetLib.IntegrationTests
{
    [TestFixture]
    public class MiddlewaresOrderModule : IModule
    {
        public void Setup(IModuleContext moduleContext)
        {
            moduleContext.PutScript("CorrectMiddlewaresOrder", _ =>
            {
                var sendingMiddlewares = _.Get<IList<IMiddleware>>("SendingMiddlewares");
                IMiddleware encryptionMiddleware =
                    sendingMiddlewares.First(middleware => middleware is EncryptionMiddleware);
                sendingMiddlewares.Remove(encryptionMiddleware);
                sendingMiddlewares.Add(encryptionMiddleware);

                var receivingMiddlewares = _.Get<IList<IMiddleware>>("ReceivingMiddlewares");
                IMiddleware decryptionMiddleware =
                    receivingMiddlewares.First(middleware => middleware is DecryptionMiddleware);
                receivingMiddlewares.Remove(decryptionMiddleware);
                receivingMiddlewares.Insert(0, decryptionMiddleware);

                IMiddleware decompressionMiddleware =
                    receivingMiddlewares.First(middleware => middleware is DecompressionMiddleware);
                receivingMiddlewares.Remove(decompressionMiddleware);
                receivingMiddlewares.Insert(1, decompressionMiddleware);
            });
        }
    }
}
