using System;
using System.Collections.Generic;
using System.Linq;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Modules.Steps;

namespace Kolyhalov.FatNetLib.Core.Modules
{
    public class StepTreeNode : IStepTreeNode
    {
        public StepTreeNode(IStep step, IStepTreeNode? parent)
        {
            Step = step;
            Parent = parent;
        }

        public IStep Step { get; }

        public IStepTreeNode? Parent { get; set; }

        public IList<IStepTreeNode> ChildNodes { get; } = new List<IStepTreeNode>();

        public IEnumerable<IStepTreeNode> ChildModuleNodes => ChildNodes.Where(node => node.Step is PutModuleStep);

        public Status Status { get; set; }

        public void Run()
        {
            Status = Status.Running;
            Step.Run();
            for (var i = 0; i < ChildNodes.Count; i++)
            {
                IStepTreeNode currentNode = ChildNodes[i];
                currentNode.Run();
                if (i >= ChildNodes.Count || currentNode != ChildNodes[i])
                    throw new FatNetLibException(
                        "Iteration was disrupted because someone wrongly changed stepTree.ChildNodes");
            }

            Status = Status.Finished;
        }

        public IStepTreeNode FindNode(StepId stepId)
        {
            IStepTreeNode parentNode = FindModuleNode(stepId.ParentModuleId);

            return FindNodeInCandidates(stepId.StepType, stepId.Qualifier, parentNode.ChildNodes)
                   ?? throw new FatNetLibException(
                       $"Can't find node by type {stepId.StepType} and qualifier {stepId.Qualifier}");
        }

        public IStepTreeNode FindModuleNode(ModuleId moduleId)
        {
            if (moduleId.Segments.Count == 0)
                throw new FatNetLibException("Can't find node. ModuleId is empty");

            IStepTreeNode? parentNode = null;

            foreach (Type currentSegment in moduleId.Segments)
            {
                IEnumerable<IStepTreeNode> candidatesParentNode = parentNode == null
                    ? new List<IStepTreeNode> { this }
                    : parentNode.ChildModuleNodes;

                parentNode = FindNodeInCandidates(typeof(PutModuleStep), currentSegment, candidatesParentNode)
                             ?? throw new FatNetLibException($"Can't find node by segment {currentSegment}");
            }

            return parentNode!;
        }

        private static IStepTreeNode? FindNodeInCandidates(
            Type requiredStepType,
            object requiredQualifier,
            IEnumerable<IStepTreeNode> candidates)
        {
            foreach (IStepTreeNode candidate in candidates)
            {
                if (candidate.Step.GetType() == requiredStepType
                    && requiredQualifier.Equals(candidate.Step.Qualifier))
                {
                    return candidate;
                }
            }

            return null;
        }

        public IEnumerable<Type> BuildAbsolutePath()
        {
            if (!(Step is PutModuleStep))
                throw new FatNetLibException("Node must be a module");

            return Parent == null
                ? Enumerable.Empty<Type>().Append((Type)Step.Qualifier)
                : Parent.BuildAbsolutePath().Append((Type)Step.Qualifier);
        }
    }

    public enum Status
    {
        ReadyToRun,
        Running,
        Finished
    }
}
