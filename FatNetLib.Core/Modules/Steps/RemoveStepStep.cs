using Kolyhalov.FatNetLib.Core.Exceptions;

namespace Kolyhalov.FatNetLib.Core.Modules.Steps
{
    public class RemoveStepStep : IStep
    {
        private readonly IStepTreeNode _rootNode;
        private readonly StepId _targetStepId;

        public RemoveStepStep(IStepTreeNode rootNode, StepId targetStepId)
        {
            _rootNode = rootNode;
            _targetStepId = targetStepId;
        }

        public object Qualifier => _targetStepId;

        public void Run()
        {
            IStepTreeNode targetNode = _rootNode.FindNode(_targetStepId);
            if (targetNode.Status != Status.ReadyToRun)
                throw new FatNetLibException("Target node has already run");

            targetNode.Parent!.ChildNodes.Remove(targetNode);
        }
    }
}
