using System;
using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Modules.Steps;

namespace Kolyhalov.FatNetLib.Core.Modules
{
    public interface IStepTreeNode
    {
        IStep Step { get; }

        IStepTreeNode? Parent { get; set; }

        IList<IStepTreeNode> ChildNodes { get; }

        IEnumerable<IStepTreeNode> ChildModuleNodes { get; }

        Status Status { get; }

        void Run();

        IStepTreeNode FindNode(StepId stepId);

        IStepTreeNode FindModuleNode(ModuleId moduleId);

        IEnumerable<Type> BuildAbsolutePath();
    }
}
