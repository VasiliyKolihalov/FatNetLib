using Kolyhalov.FatNetLib.Core.Exceptions;
using static Kolyhalov.FatNetLib.Core.Modules.Steps.StepId;

namespace Kolyhalov.FatNetLib.Core.Modules.Steps
{
    public class MoveStepAfterStep : IStep
    {
        private readonly IStepTreeNode _rootNode;
        private readonly StepId _targetStepId;
        private readonly StepId _placeStepId;

        public MoveStepAfterStep(IStepTreeNode rootNode, StepId targetStepId, StepId placeStepId)
        {
            _rootNode = rootNode;
            _targetStepId = targetStepId;
            _placeStepId = placeStepId;
        }

        public object Qualifier => EmptyQualifier;

        public void Run()
        {
            IStepTreeNode targetNode = _rootNode.FindNode(_targetStepId);
            if (targetNode.Status != Status.ReadyToRun)
                throw new FatNetLibException("Target node has already run");
            IStepTreeNode placeNode = _rootNode.FindNode(_placeStepId);
            if (placeNode.Status != Status.ReadyToRun)
                throw new FatNetLibException("Place node has already run");

            targetNode.Parent!.ChildNodes.Remove(targetNode);
            int placeIndex = placeNode.Parent!.ChildNodes.IndexOf(placeNode);
            placeNode.Parent!.ChildNodes.Insert(placeIndex + 1, targetNode);
            targetNode.Parent = placeNode.Parent;
        }
    }
}
