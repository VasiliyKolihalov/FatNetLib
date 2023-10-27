using System;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Modules;
using Kolyhalov.FatNetLib.Core.Modules.Steps;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests.Modules.Steps;

public class RemoveModuleStepTests
{
    [Test]
    public void Run_OnStepTree_ModuleIsRemoved()
    {
        // Arrange
        var root = new StepTreeNode(new PutModuleStep(new TestModule()), parent: null!);
        var targetNode = new StepTreeNode(new PutModuleStep(new TestModule()), parent: root);
        root.ChildNodes.Add(targetNode);

        var targetModuleId = new ModuleId(typeof(TestModule), typeof(TestModule));
        var step = new RemoveModuleStep(root, targetModuleId);

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
}
