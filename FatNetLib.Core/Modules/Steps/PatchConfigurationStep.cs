using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Storages;

namespace Kolyhalov.FatNetLib.Core.Modules.Steps
{
    public class PatchConfigurationStep : IStep
    {
        private readonly Configuration _configurationPatch;
        private readonly IDependencyContext _dependencyContext;

        public PatchConfigurationStep(Configuration configurationPatch, IDependencyContext dependencyContext)
        {
            _configurationPatch = configurationPatch;
            _dependencyContext = dependencyContext;
        }

        public object Qualifier => StepId.EmptyQualifier;

        public void Run()
        {
            _dependencyContext.Get<Configuration>().Patch(_configurationPatch);
        }
    }
}
