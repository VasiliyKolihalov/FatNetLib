using System;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Modules;
using Kolyhalov.FatNetLib.Core.Modules.Steps;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests.Modules.Steps;

public class ReplaceOldStepStepTests
{
    [Test]
    public void Run_OnStepTree_StepIsReplaced()
    {
        // Arrange
        var root = new StepTreeNode(new PutModuleStep(new TestModule()), parent: null!);
        var targetNode = new StepTreeNode(new TestStep("qualifier-1"), root);
        var oldNode = new StepTreeNode(new TestStep("qualifier-2"), root);
        root.ChildNodes.Add(targetNode);
        root.ChildNodes.Add(oldNode);

        var targetStepId = new StepId(new ModuleId(typeof(TestModule)), typeof(TestStep), "qualifier-1");
        var oldStepId = new StepId(new ModuleId(typeof(TestModule)), typeof(TestStep), "qualifier-2");
        var step = new ReplaceOldStepStep(root, targetStepId, oldStepId);

        // Act
        step.Run();

        // Assert
        root.ChildNodes.Should().Equal(targetNode);
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
