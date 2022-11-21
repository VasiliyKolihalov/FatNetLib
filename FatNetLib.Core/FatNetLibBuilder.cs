using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Modules;
using Kolyhalov.FatNetLib.Core.Recorders;
using Kolyhalov.FatNetLib.Core.Storages;

namespace Kolyhalov.FatNetLib.Core
{
    public class FatNetLibBuilder
    {
        private readonly IDependencyContext _dependencyContext = new DependencyContext();
        private readonly IEndpointsStorage _endpointsStorage = new EndpointsStorage();

        public IList<IModule> Modules { get; set; } = new List<IModule>();

        public Configuration? ConfigurationPatch { private get; set; } = null!;

        public IEndpointRecorder Endpoints { get; }

        public FatNetLibBuilder()
        {
            Endpoints = new EndpointRecorder(_endpointsStorage);
        }

        public FatNetLib BuildAndRun()
        {
            var rootModule = new FatNetLibBuilderModule(Modules, _endpointsStorage, Endpoints, ConfigurationPatch);
            new ModuleContext(rootModule, _dependencyContext)
                .Build();

            var fatNetLib = _dependencyContext.Get<FatNetLib>();
            fatNetLib.Run();
            return fatNetLib;
        }
    }
}
