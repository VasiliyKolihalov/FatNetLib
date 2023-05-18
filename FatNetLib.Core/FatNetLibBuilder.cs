using System;
using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Modules;
using Kolyhalov.FatNetLib.Core.Recorders;
using Kolyhalov.FatNetLib.Core.Storages;
using IModule = Kolyhalov.FatNetLib.Core.Modules.IModule;

namespace Kolyhalov.FatNetLib.Core
{
    public class FatNetLibBuilder
    {
        private readonly IDependencyContext _dependencyContext = new DependencyContext();
        private readonly IEndpointsStorage _endpointsStorage = new EndpointsStorage();

        public IList<IModule> Modules { get; set; } = new List<IModule>();

        public Configuration? ConfigurationPatch { private get; set; }

        public PackageSchema? DefaultPackageSchemaPatch { private get; set; }

        public IEnumerable<Type>? SendingMiddlewaresOrder { private get; set; }

        public IEnumerable<Type>? ReceivingMiddlewaresOrder { private get; set; }

        public IEndpointRecorder Endpoints { get; }

        public FatNetLibBuilder()
        {
            Endpoints = new EndpointRecorder(_endpointsStorage);
        }

        public FatNetLib BuildAndRun()
        {
            var rootModule = new RootModule(
                Modules,
                _endpointsStorage,
                Endpoints,
                ConfigurationPatch,
                DefaultPackageSchemaPatch,
                SendingMiddlewaresOrder,
                ReceivingMiddlewaresOrder);

            new ModuleBuilder(rootModule, _dependencyContext)
                .BuildAndRun();

            var fatNetLib = _dependencyContext.Get<FatNetLib>();
            fatNetLib.Run();
            return fatNetLib;
        }
    }
}
