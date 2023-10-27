using System;
using System.Collections.Generic;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Modules;
using Kolyhalov.FatNetLib.Core.Modules.Steps;
using Moq;
using NUnit.Framework;
using static Kolyhalov.FatNetLib.Core.Modules.Status;

namespace Kolyhalov.FatNetLib.Core.Tests.Modules;

public class StepTreeNodeTests
{
    [Test]
    public void Run_StepTree_RunAllSteps()
    {
        // Arrange
        var rootStep = new Mock<IStep>();
        var childStep = new Mock<IStep>();
        var root = new StepTreeNode(rootStep.Object, parent: null!);
        var childNode = new StepTreeNode(childStep.Object, root);
        root.ChildNodes.Add(childNode);

        // Act
        root.Run();

        // Assert
        rootStep.Verify(_ => _.Run());
        childStep.Verify(_ => _.Run());
        root.Status.Should().Be(Finished);
        childNode.Status.Should().Be(Finished);
    }

    [Test]
    public void Run_SameChildModulesType_Throw()
    {
        var root = new StepTreeNode(new Mock<IStep>().Object, parent: null!);
        root.ChildNodes.Add(new StepTreeNode(new PutModuleStep(new ModuleA()), root));
        root.ChildNodes.Add(new StepTreeNode(new PutModuleStep(new ModuleA()), root));

        // Act
        var act = () => root.Run();

        // Assert
        act.Should().Throw<FatNetLibException>()
            .WithMessage("A module with type Kolyhalov.FatNetLib.Core.Tests.Modules.StepTreeNodeTests+ModuleA " +
                         "is already registered in this module");
    }

    [Test]
    public void FindNode()
    {
        // Arrange
        var rootStep = new PutModuleStep(new ModuleA());
        var childStep = new TestStep("qualifier-1");
        var root = new StepTreeNode(rootStep, parent: null!);
        var childNode = new StepTreeNode(childStep, root);
        root.ChildNodes.Add(childNode);
        var childStepId = new StepId(new ModuleId(typeof(ModuleA)), typeof(TestStep), qualifier: "qualifier-1");

        // Act
        IStepTreeNode foundNode = root.FindNode(childStepId);

        // Assert
        foundNode.Should().Be(childNode);
    }

    [Test]
    public void FindModuleNode()
    {
        // Arrange
        var rootStep = new PutModuleStep(new ModuleA());
        var childStep = new PutModuleStep(new ModuleB());
        var root = new StepTreeNode(rootStep, parent: null!);
        var childNode = new StepTreeNode(childStep, root);
        root.ChildNodes.Add(childNode);
        var childStepId = new ModuleId(typeof(ModuleA), typeof(ModuleB));

        // Act
        IStepTreeNode foundNode = root.FindModuleNode(childStepId);

        // Assert
        foundNode.Should().Be(childNode);
    }

    [Test]
    public void BuildAbsolutePath()
    {
        // Arrange
        var rootStep = new PutModuleStep(new ModuleA());
        var childStep = new PutModuleStep(new ModuleB());
        var root = new StepTreeNode(rootStep, parent: null!);
        var childNode = new StepTreeNode(childStep, root);
        root.ChildNodes.Add(childNode);

        // Act
        IEnumerable<Type> path = childNode.BuildAbsolutePath();

        // Assert
        path.Should().Equal(typeof(ModuleA), typeof(ModuleB));
    }

    private class ModuleA : IModule
    {
        public void Setup(IModuleContext moduleContext)
        {
            throw new InvalidOperationException("This test method shouldn't be called");
        }
    }

    private class ModuleB : IModule
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
