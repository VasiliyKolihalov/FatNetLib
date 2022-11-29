using System;
using System.Collections.Generic;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Modules;
using Kolyhalov.FatNetLib.Core.Modules.Steps;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests.Modules
{
    public class FindStepModuleContextTests
    {
        private static readonly IModuleStep TargetStep = new TestStep(new StepId(
            parentModuleType: typeof(TestModule),
            stepType: typeof(TestStep),
            qualifier: "test-step"));

        private static readonly IModuleStep AnotherStep = new TestStep(new StepId(
            parentModuleType: typeof(TestModule),
            stepType: typeof(TestStep),
            qualifier: "another-step"));

        private static readonly IModuleStep UnknownStep = new TestStep(new StepId(
            parentModuleType: typeof(TestModule),
            stepType: typeof(TestStep),
            qualifier: "unknown-step"));

        [Test]
        public void AndMoveBeforeStep_CorrectCase_MoveStep()
        {
            // Arrange
            var steps = new List<IModuleStep> { AnotherStep, TargetStep };
            var context = new FindStepContext(TargetStep.Id, steps, context: null!);

            // Act
            context.AndMoveBeforeStep(AnotherStep.Id);

            // Assert
            steps.Should().BeEquivalentTo(new[] { TargetStep, AnotherStep }, _ => _.WithStrictOrdering());
        }

        [Test]
        public void AndMoveBeforeStep_TargetIsUnknown_Throw()
        {
            // Arrange
            var steps = new List<IModuleStep> { AnotherStep };
            var context = new FindStepContext(UnknownStep.Id, steps, context: null!);

            // Act
            Action act = () => context.AndMoveBeforeStep(AnotherStep.Id);

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Step with id StepId(*, Qualifier: unknown-step) not found");
        }

        [Test]
        public void AndMoveBeforeStep_BeforeStepIsUnknown_Throw()
        {
            // Arrange
            var steps = new List<IModuleStep> { TargetStep };
            var context = new FindStepContext(UnknownStep.Id, steps, context: null!);

            // Act
            Action act = () => context.AndMoveBeforeStep(UnknownStep.Id);

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Step with id StepId(*, Qualifier: unknown-step) not found");
        }

        [Test]
        public void AndMoveAfterStep_CorrectCase_MoveStep()
        {
            // Arrange
            var steps = new List<IModuleStep> { TargetStep, AnotherStep };
            var context = new FindStepContext(TargetStep.Id, steps, context: null!);

            // Act
            context.AndMoveAfterStep(AnotherStep.Id);

            // Assert
            steps.Should().BeEquivalentTo(new[] { AnotherStep, TargetStep }, _ => _.WithStrictOrdering());
        }

        [Test]
        public void AndMoveAfterStep_TargetIsUnknown_Throw()
        {
            // Arrange
            var steps = new List<IModuleStep> { AnotherStep };
            var context = new FindStepContext(UnknownStep.Id, steps, context: null!);

            // Act
            Action act = () => context.AndMoveAfterStep(AnotherStep.Id);

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Step with id StepId(*, Qualifier: unknown-step) not found");
        }

        [Test]
        public void AndMoveAfterStep_BeforeStepIsUnknown_Throw()
        {
            // Arrange
            var steps = new List<IModuleStep> { TargetStep };
            var context = new FindStepContext(UnknownStep.Id, steps, context: null!);

            // Act
            Action act = () => context.AndMoveAfterStep(UnknownStep.Id);

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Step with id StepId(*, Qualifier: unknown-step) not found");
        }

        [Test]
        public void AndReplaceOldStep_CorrectCase_ReplaceStep()
        {
            // Arrange
            var steps = new List<IModuleStep> { TargetStep, AnotherStep };
            var context = new FindStepContext(TargetStep.Id, steps, context: null!);

            // Act
            context.AndReplaceOld(AnotherStep.Id);

            // Assert
            steps.Should().BeEquivalentTo(new[] { TargetStep });
        }

        [Test]
        public void AndReplaceOldStep_TargetIsUnknown_Throw()
        {
            // Arrange
            var steps = new List<IModuleStep> { AnotherStep };
            var context = new FindStepContext(UnknownStep.Id, steps, context: null!);

            // Act
            Action act = () => context.AndReplaceOld(AnotherStep.Id);

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Step with id StepId(*, Qualifier: unknown-step) not found");
        }

        [Test]
        public void AndReplaceOldStep_BeforeStepIsUnknown_Throw()
        {
            // Arrange
            var steps = new List<IModuleStep> { TargetStep };
            var context = new FindStepContext(UnknownStep.Id, steps, context: null!);

            // Act
            Action act = () => context.AndReplaceOld(UnknownStep.Id);

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Step with id StepId(*, Qualifier: unknown-step) not found");
        }

        [Test]
        public void AndRemoveIt_CorrectCase_RemoveStep()
        {
            // Arrange
            var steps = new List<IModuleStep> { TargetStep, AnotherStep };
            var context = new FindStepContext(TargetStep.Id, steps, context: null!);

            // Act
            context.AndRemoveIt();

            // Assert
            steps.Should().BeEquivalentTo(new[] { AnotherStep });
        }

        [Test]
        public void AndRemoveIt_TargetIsUnknown_Throw()
        {
            // Arrange
            var steps = new List<IModuleStep> { AnotherStep };
            var context = new FindStepContext(UnknownStep.Id, steps, context: null!);

            // Act
            Action act = () => context.AndRemoveIt();

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Step with id StepId(*, Qualifier: unknown-step) not found");
        }
    }
}
