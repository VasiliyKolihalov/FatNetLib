using System;
using System.Collections.Generic;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Modules;
using Kolyhalov.FatNetLib.Core.Modules.Steps;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests.Modules
{
    public class FindModuleModuleContextTests
    {
        private static readonly IModuleStep ExternalStep = new TestStep(new StepId(
            parentModuleType: typeof(ParentTestModule),
            stepType: typeof(TestStep),
            qualifier: "external-step"));

        private static readonly IModuleStep InternalStep = new TestStep(new StepId(
            parentModuleType: typeof(TestModule),
            stepType: typeof(TestStep),
            qualifier: "internal-step"));

        private static readonly IModuleStep BeginModuleStep = new BeginModuleStep(new ModuleId(
            targetType: typeof(TestModule),
            parentType: typeof(ParentTestModule)));

        private static readonly IModuleStep EndModuleStep = new EndModuleStep(new ModuleId(
            targetType: typeof(TestModule),
            parentType: typeof(ParentTestModule)));

        private static readonly IModuleStep BeginReplaceModuleStep = new BeginModuleStep(new ModuleId(
            targetType: typeof(ReplaceTestModule),
            parentType: typeof(ReplaceParentTestModule)));

        private static readonly IModuleStep EndReplaceModuleStep = new EndModuleStep(new ModuleId(
            targetType: typeof(ReplaceTestModule),
            parentType: typeof(ReplaceParentTestModule)));

        private static readonly IModuleStep InternalReplaceStep = new TestStep(new StepId(
            parentModuleType: typeof(ReplaceTestModule),
            stepType: typeof(TestStep),
            qualifier: "internal-replace-step"));

        [Test]
        public void AndRemoveIt_CorrectCase_RemoveModule()
        {
            // Arrange
            var steps = new List<IModuleStep>
                { ExternalStep, BeginModuleStep, InternalStep, EndModuleStep, ExternalStep };
            var context = new FindModuleContext(
                new ModuleId(
                    typeof(ParentTestModule),
                    typeof(TestModule)),
                steps,
                context: null!);

            // Act
            context.AndRemoveIt();

            // Assert
            steps.Should().BeEquivalentTo(new[] { ExternalStep, ExternalStep });
        }

        [Test]
        public void AndRemoveModule_UnknownModule_Throw()
        {
            // Arrange
            var steps = new List<IModuleStep>
                { ExternalStep };
            var context = new FindModuleContext(
                new ModuleId(
                    typeof(ParentTestModule),
                    typeof(TestModule)),
                steps,
                context: null!);

            // Act
            Action act = () => context.AndRemoveIt();

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Module *.TestModule not found");
        }

        [Test]
        public void AndReplaceOld_CorrectCase_ReplaceOldModule()
        {
            // Arrange
            var steps = new List<IModuleStep>
            {
                ExternalStep,
                BeginReplaceModuleStep,
                InternalReplaceStep,
                EndReplaceModuleStep,
                ExternalStep,
                BeginModuleStep,
                InternalStep,
                EndModuleStep
            };

            var context = new FindModuleContext(
                new ModuleId(
                    typeof(ParentTestModule),
                    typeof(TestModule)),
                steps,
                context: null!);

            // Act
            context.AndReplaceOld(new ModuleId(
                parentType: typeof(ReplaceParentTestModule),
                targetType: typeof(ReplaceTestModule)));

            // Assert
            steps.Should().BeEquivalentTo(new[]
            {
                ExternalStep,
                BeginModuleStep,
                InternalStep,
                EndModuleStep,
                ExternalStep
            });
        }

        [Test]
        public void AndReplaceOld_UnknownReplaceModule_Throw()
        {
            // Arrange
            var steps = new List<IModuleStep>
            {
                ExternalStep,
                BeginModuleStep,
                InternalStep,
                EndModuleStep
            };

            var context = new FindModuleContext(
                new ModuleId(
                    typeof(ParentTestModule),
                    typeof(TestModule)),
                steps,
                context: null!);

            // Act
            Action act = () => context.AndReplaceOld(new ModuleId(
                parentType: typeof(ReplaceParentTestModule),
                targetType: typeof(ReplaceTestModule)));

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Module *.FindModuleModuleContextTests+ReplaceTestModule not found");
        }

        [Test]
        public void AndReplaceOld_UnknownModule_Throw()
        {
            // Arrange
            var steps = new List<IModuleStep>
            {
                ExternalStep,
                BeginReplaceModuleStep,
                InternalReplaceStep,
                EndReplaceModuleStep,
                ExternalStep,
            };

            var context = new FindModuleContext(
                new ModuleId(
                    typeof(ParentTestModule),
                    typeof(TestModule)),
                steps,
                context: null!);

            // Act
            Action act = () => context.AndReplaceOld(new ModuleId(
                parentType: typeof(ReplaceParentTestModule),
                targetType: typeof(ReplaceTestModule)));

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Module *.TestModule not found");
        }

        private class ParentTestModule
        {
        }

        private class ReplaceParentTestModule
        {
        }

        private class ReplaceTestModule
        {
        }
    }
}
