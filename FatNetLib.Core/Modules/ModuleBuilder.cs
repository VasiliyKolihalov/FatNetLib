using Kolyhalov.FatNetLib.Core.Storages;

namespace Kolyhalov.FatNetLib.Core.Modules
{
    public class ModuleBuilder
    {
        private readonly IModule _rootModule;
        private readonly IDependencyContext _dependencyContext;

        public ModuleBuilder(IModule rootModule, IDependencyContext dependencyContext)
        {
            _rootModule = rootModule;
            _dependencyContext = dependencyContext;
        }

        public void BuildAndRun()
        {
            IStepTreeNode stepTree = new StepTreeBuilder(_rootModule, _dependencyContext).Build();
            stepTree.Run();
        }
    }
}
