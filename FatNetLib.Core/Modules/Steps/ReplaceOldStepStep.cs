using Kolyhalov.FatNetLib.Core.Exceptions;
using static Kolyhalov.FatNetLib.Core.Modules.Steps.StepId;

namespace Kolyhalov.FatNetLib.Core.Modules.Steps
{
    public class ReplaceOldStepStep : IStep
    {
        private readonly IStepTreeNode _rootNode;
        private readonly StepId _targetStepId;
        private readonly StepId _oldStepId;

        public ReplaceOldStepStep(IStepTreeNode rootNode, StepId targetStepId, StepId oldStepId)
        {
            _rootNode = rootNode;
            _targetStepId = targetStepId;
            _oldStepId = oldStepId;
        }

        public object Qualifier => EmptyQualifier;

        public void Run()
        {
            IStepTreeNode targetNode = _rootNode.FindNode(_targetStepId);
            if (targetNode.Status != Status.ReadyToRun)
                throw new FatNetLibException("Target node has already run");
            IStepTreeNode oldNode = _rootNode.FindNode(_oldStepId);
            if (oldNode.Status != Status.ReadyToRun)
                throw new FatNetLibException("Old node has already run");

            targetNode.Parent!.ChildNodes.Remove(targetNode);
            int placeIndex = oldNode.Parent!.ChildNodes.IndexOf(oldNode);
            oldNode.Parent!.ChildNodes.Insert(placeIndex, targetNode);
            targetNode.Parent = oldNode.Parent;
            oldNode.Parent!.ChildNodes.Remove(oldNode);
        }
    }
}
