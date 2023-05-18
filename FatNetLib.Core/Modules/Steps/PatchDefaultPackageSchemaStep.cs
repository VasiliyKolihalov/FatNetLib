using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Storages;

namespace Kolyhalov.FatNetLib.Core.Modules.Steps
{
    public class PatchDefaultPackageSchemaStep : IStep
    {
        private readonly PackageSchema _defaultPackageSchemaPatch;
        private readonly IDependencyContext _dependencyContext;

        public PatchDefaultPackageSchemaStep(
            PackageSchema defaultPackageSchemaPatch,
            IDependencyContext dependencyContext)
        {
            _defaultPackageSchemaPatch = defaultPackageSchemaPatch;
            _dependencyContext = dependencyContext;
        }

        public object Qualifier => StepId.EmptyQualifier;

        public void Run()
        {
            _dependencyContext.Get<PackageSchema>("DefaultPackageSchema").Patch(_defaultPackageSchemaPatch);
        }
    }
}
