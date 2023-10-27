using System;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Modules;
using Kolyhalov.FatNetLib.Core.Modules.Steps;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests.Modules.Steps;

public class RemoveStepStepTests
{
    [Test]
    public void Run_OnStepTree_StepIsRemoved()
    {
        // Arrange
        var root = new StepTreeNode(new PutModuleStep(new TestModule()), parent: null!);
        var targetNode = new StepTreeNode(new TestStep("qualifier-1"), parent: root);
        root.ChildNodes.Add(targetNode);

        var targetModuleId = new StepId(new ModuleId(typeof(TestModule)), typeof(TestStep), "qualifier-1");
        var step = new RemoveStepStep(root, targetModuleId);

        // Act
        step.Run();

        // Assert
        root.ChildNodes.Should().BeEmpty();
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
