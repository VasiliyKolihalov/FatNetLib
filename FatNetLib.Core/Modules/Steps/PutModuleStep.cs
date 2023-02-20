namespace Kolyhalov.FatNetLib.Core.Modules.Steps
{
    public class PutModuleStep : IStep
    {
        public PutModuleStep(IModule module)
        {
            Module = module;
        }

        public IModule Module { get; }

        public object Qualifier => Module.GetType();

        public void Run()
        {
            // No actions required
        }
    }
}
