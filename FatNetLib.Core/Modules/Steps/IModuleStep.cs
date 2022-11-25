namespace Kolyhalov.FatNetLib.Core.Modules.Steps
{
    public interface IModuleStep
    {
        public StepId Id { get; }

        public void Run();

        public IModuleStep CopyWithNewId(StepId newId);
    }
}
