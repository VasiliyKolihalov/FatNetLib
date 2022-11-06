﻿using Microsoft.Extensions.Logging;

namespace Kolyhalov.FatNetLib.Modules.Encryption;

public class ClientEncryptionModule : IModule
{
    public void Setup(ModuleContext moduleContext)
    {
        var logger = moduleContext.DependencyContext.Get<ILogger>();
        var encryptionMiddleware = new EncryptionMiddleware(maxNonEncodingPeriod: 2, logger);
        var decryptionMiddleware = new DecryptionMiddleware(maxNonDecodingPeriod: 2, logger);
        moduleContext.SendingMiddlewares.Add(encryptionMiddleware);
        moduleContext.ReceivingMiddlewares.Insert(0, decryptionMiddleware);
        moduleContext.EndpointRecorder.AddController(
            new ClientEncryptionController(encryptionMiddleware, decryptionMiddleware));
    }

    public IList<IModule>? ChildModules { get; } = new List<IModule>();
}
