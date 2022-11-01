﻿using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Endpoints;
using Kolyhalov.FatNetLib.Middlewares;
using Kolyhalov.FatNetLib.Modules;
using Microsoft.Extensions.Logging;

namespace Kolyhalov.FatNetLib;

public class FatNetLibBuilder
{
    public Configuration? ConfigurationPatch { private get; init; } = null!;
    public PackageSchema? PackageSchemaPatch { private get; init; } = null!; // Todo: move into configuration
    public ILogger? Logger { private get; init; } = null!;

    public IModulesRecorder Modules { get; } = new ModulesRecorder();
    public IEndpointRecorder Endpoints { get; }
    public IList<IMiddleware> SendingMiddlewares { get; } = new List<IMiddleware>();
    public IList<IMiddleware> ReceivingMiddlewares { get; } = new List<IMiddleware>();

    private readonly IDependencyContext _dependencyContext = new DependencyContext();

    public FatNetLibBuilder()
    {
        CreateEndpointsStorage();
        CreateEndpointRecorder();
        Endpoints = _dependencyContext.Get<IEndpointRecorder>();
    }

    public FatNetLib Build()
    {
        PutMiddlewares();

        var modulesContext = new ModuleContext(_dependencyContext);
        Modules.Setup(modulesContext);

        PutLogger();
        PatchConfiguration();
        PatchPackageSchema();

        return new FatNetLib(_dependencyContext.Get<IClient>(), _dependencyContext.Get<INetEventListener>());
    }

    private void CreateEndpointsStorage()
    {
        _dependencyContext.Put<IEndpointsStorage>(_ => new EndpointsStorage());
    }

    private void CreateEndpointRecorder()
    {
        _dependencyContext.Put<IEndpointRecorder>(context => new EndpointRecorder(context.Get<IEndpointsStorage>()));
    }

    private void PutMiddlewares()
    {
        _dependencyContext.Put("SendingMiddlewares", _ => SendingMiddlewares);
        _dependencyContext.Put("ReceivingMiddlewares", _ => ReceivingMiddlewares);
    }

    private void PutLogger()
    {
        if (Logger == null)
            return;

        _dependencyContext.Put<ILogger>(_ => Logger);
    }

    private void PatchConfiguration()
    {
        if (ConfigurationPatch == null)
            return;

        var configuration = _dependencyContext.Get<Configuration>();
        configuration.Patch(ConfigurationPatch);
    }

    private void PatchPackageSchema()
    {
        if (PackageSchemaPatch == null)
            return;

        _dependencyContext.Get<PackageSchema>("DefaultPackageSchema").Patch(PackageSchemaPatch);
    }
}