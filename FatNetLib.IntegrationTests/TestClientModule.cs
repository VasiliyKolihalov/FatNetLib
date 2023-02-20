using System.Collections.Generic;
using System.Linq;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Middlewares;
using Kolyhalov.FatNetLib.Core.Modules;
using Kolyhalov.FatNetLib.Core.Modules.Defaults;
using Kolyhalov.FatNetLib.Core.Modules.Defaults.Client;
using Kolyhalov.FatNetLib.Json;
using Kolyhalov.FatNetLib.MicrosoftLogging;
using NUnit.Framework;
using static Kolyhalov.FatNetLib.Core.Modules.ModuleId.Pointers;
using static Kolyhalov.FatNetLib.Core.Modules.Steps.StepType;
using IMicrosoftLogger = Microsoft.Extensions.Logging.ILogger;

namespace Kolyhalov.FatNetLib.IntegrationTests
{
    [TestFixture]
    public class TestClientModule : IModule
    {
        public void Setup(IModuleContext moduleContext)
        {
            CorrectLogger(moduleContext);
            moduleContext
                .PutModule(new DefaultClientModule())
                .PutModule(new JsonModule())
                .PutModule(new MicrosoftLoggerModule());
            CorrectMiddlewaresOrder(moduleContext);
        }

        private static void CorrectLogger(IModuleContext moduleContext)
        {
            moduleContext
                .FindStep(
                    parent: ThisModule / typeof(MicrosoftLoggerModule),
                    step: PutDependency,
                    qualifier: typeof(ILogger))
                .AndReplaceOld(
                    parent: ThisModule / typeof(DefaultClientModule) / typeof(DefaultCommonModule),
                    step: PutDependency,
                    qualifier: typeof(ILogger));

            moduleContext
                .FindStep(
                    parent: ThisModule / typeof(MicrosoftLoggerModule),
                    step: PutDependency,
                    qualifier: typeof(IMicrosoftLogger))
                .AndMoveBeforeStep(
                    parent: ThisModule / typeof(DefaultClientModule) / typeof(DefaultCommonModule),
                    step: PutDependency,
                    qualifier: typeof(ILogger));
        }

        private static void CorrectMiddlewaresOrder(IModuleContext moduleContext)
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
            });
        }
    }
}
