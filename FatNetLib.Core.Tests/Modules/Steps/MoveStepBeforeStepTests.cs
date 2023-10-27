using System;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Modules;
using Kolyhalov.FatNetLib.Core.Modules.Steps;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests.Modules.Steps;

public class MoveStepBeforeStepTests
{
    [Test]
    public void Run_OnStepTree_StepIsMoved()
    {
        // Arrange
        var root = new StepTreeNode(new PutModuleStep(new TestModule()), parent: null!);
        var placeNode = new StepTreeNode(new TestStep("qualifier-1"), root);
        var targetNode = new StepTreeNode(new TestStep("qualifier-2"), root);
        root.ChildNodes.Add(placeNode);
        root.ChildNodes.Add(targetNode);

        var placeStepId = new StepId(new ModuleId(typeof(TestModule)), typeof(TestStep), "qualifier-1");
        var targetStepId = new StepId(new ModuleId(typeof(TestModule)), typeof(TestStep), "qualifier-2");
        var step = new MoveStepBeforeStep(root, targetStepId, placeStepId);

        // Act
        step.Run();

        // Assert
        root.ChildNodes.Should().Equal(targetNode, placeNode);
    }

    private class TestModule : IModule
    {
        public void Setup(IModuleContext moduleContext)
        {
            throw new InvalidOperationException("This test method shouldn't be called");
        }
    }

    private class TestStep : IStep
    {
        public TestStep(object qualifier)
        {
            Qualifier = qualifier;
        }

        public object Qualifier { get; }

        public void Run()
        {
            throw new InvalidOperationException("This test method shouldn't be called");
        }
    }
}
