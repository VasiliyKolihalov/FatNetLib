using System;
using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Couriers;
using Kolyhalov.FatNetLib.Core.Recorders;
using Kolyhalov.FatNetLib.Core.Storages;
using Kolyhalov.FatNetLib.Core.Subscribers;

namespace Kolyhalov.FatNetLib.Core.Modules
{
    public class RootModule : IModule
    {
        private readonly IList<IModule> _modules;
        private readonly IEndpointsStorage _endpointsStorage;
        private readonly IEndpointRecorder _endpointsRecorder;
        private readonly Configuration? _configurationPatch;
        private readonly PackageSchema? _defaultPackageSchemaPatch;
        private readonly IEnumerable<Type>? _sendingMiddlewaresOrder;
        private readonly IEnumerable<Type>? _receivingMiddlewaresOrder;

        public RootModule(
            IList<IModule> modules,
            IEndpointsStorage endpointsStorage,
            IEndpointRecorder endpointsRecorder,
            Configuration? configurationPatch,
            PackageSchema? defaultPackageSchemaPatch,
            IEnumerable<Type>? sendingMiddlewaresOrder,
            IEnumerable<Type>? receivingMiddlewaresOrder)
        {
            _modules = modules;
            _endpointsStorage = endpointsStorage;
            _endpointsRecorder = endpointsRecorder;
            _configurationPatch = configurationPatch;
            _defaultPackageSchemaPatch = defaultPackageSchemaPatch;
            _sendingMiddlewaresOrder = sendingMiddlewaresOrder;
            _receivingMiddlewaresOrder = receivingMiddlewaresOrder;
        }

        public void Setup(IModuleContext moduleContext)
        {
            moduleContext
                .PutDependency(_ => _endpointsStorage)
                .PutDependency(_ => _endpointsRecorder)
                .PutModules(_modules);

            if (_configurationPatch != null)
                moduleContext.PatchConfiguration(_configurationPatch);

            if (_defaultPackageSchemaPatch != null)
                moduleContext.PatchDefaultPackageSchema(_defaultPackageSchemaPatch);

            if (_sendingMiddlewaresOrder != null)
                moduleContext.SortSendingMiddlewares(_sendingMiddlewaresOrder);

            if (_receivingMiddlewaresOrder != null)
                moduleContext.SortReceivingMiddlewares(_receivingMiddlewaresOrder);

            moduleContext.PutDependency(_ => new FatNetLib(
                _.Get<ICourier>(),
                _.Get<INetEventListener>()));
        }
    }
}
