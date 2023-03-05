using Kolyhalov.FatNetLib.Core.Exceptions;

namespace Kolyhalov.FatNetLib.Core.Modules.Steps
{
    public class RemoveModuleStep : IStep
    {
        private readonly IStepTreeNode _rootNode;
        private readonly ModuleId _targetModuleId;

        public RemoveModuleStep(IStepTreeNode rootNode, ModuleId targetModuleId)
        {
            _rootNode = rootNode;
            _targetModuleId = targetModuleId;
        }

        public object Qualifier => _targetModuleId;

        public void Run()
        {
            IStepTreeNode targetNode = _rootNode.FindModuleNode(_targetModuleId);
            if (targetNode.Status != Status.ReadyToRun)
                throw new FatNetLibException("Target node has already run");

            targetNode.Parent!.ChildNodes.Remove(targetNode);
        }
    }
}
