namespace Kolyhalov.FatNetLib.Core.Modules.Steps
{
    public interface IStep
    {
        object Qualifier { get; }

        void Run();
    }
}
