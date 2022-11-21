using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Recorders;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Subscribers;

namespace Kolyhalov.FatNetLib.Core.Modules
{
    public class FatNetLibBuilderModule : IModule
    {
        private readonly IList<IModule> _modules;
        private readonly IEndpointsStorage _endpointsStorage;
        private readonly IEndpointRecorder _endpointsRecorder;
        private readonly Configuration? _configurationPatch;

        public FatNetLibBuilderModule(
            IList<IModule> modules,
            IEndpointsStorage endpointsStorage,
            IEndpointRecorder endpointsRecorder,
            Configuration? configurationPatch)
        {
            _modules = modules;
            _endpointsStorage = endpointsStorage;
            _endpointsRecorder = endpointsRecorder;
            _configurationPatch = configurationPatch;
        }

        public void Setup(IModuleContext moduleContext)
        {
            moduleContext
                .PutDependency(_ => _endpointsStorage)
                .PutDependency(_ => _endpointsRecorder)
                .PutModules(_modules)
                .PutScript("PatchConfiguration", _ =>
                {
                    if (_configurationPatch is null)
                        return;

                    var configuration = _.Get<Configuration>();
                    configuration.Patch(_configurationPatch);

                    if (configuration.DefaultSchemaPatch != null)
                        _.Get<PackageSchema>("DefaultPackageSchema").Patch(configuration.DefaultSchemaPatch);
                })
                .PutDependency(_ => new FatNetLib(
                    _.Get<ICourier>(),
                    _.Get<INetEventListener>()));
        }
    }
}
