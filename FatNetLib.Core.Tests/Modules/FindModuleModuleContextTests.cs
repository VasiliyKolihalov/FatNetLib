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
            inModuleId: "external-step"));

        private static readonly IModuleStep InternalStep = new TestStep(new StepId(
            parentModuleType: typeof(TestModule),
            stepType: typeof(TestStep),
            inModuleId: "internal-step"));

        private static readonly IModuleStep BeginModuleStep = new BeginModuleStep(
            moduleType: typeof(TestModule),
            parentModuleType: typeof(ParentTestModule));

        private static readonly IModuleStep EndModuleStep = new EndModuleStep(
            moduleType: typeof(TestModule),
            parentModuleType: typeof(ParentTestModule));

        [Test]
        public void AndRemoveModule_CorrectCase_RemoveModule()
        {
            // Arrange
            var steps = new List<IModuleStep>
                { ExternalStep, BeginModuleStep, InternalStep, EndModuleStep, ExternalStep };
            var context = new FindModuleModuleContext(typeof(ParentTestModule), steps, context: null!);

            // Act
            context.AndRemoveModule(typeof(TestModule));

            // Assert
            steps.Should().BeEquivalentTo(new[] { ExternalStep, ExternalStep });
        }

        [Test]
        public void AndRemoveModule_UnknownModule_RemoveModule()
        {
            // Arrange
            var steps = new List<IModuleStep>
                { ExternalStep };
            var context = new FindModuleModuleContext(typeof(ParentTestModule), steps, context: null!);

            // Act
            Action act = () => context.AndRemoveModule(typeof(TestModule));

            // Assert
            act.Should().Throw<FatNetLibException>()
                .WithMessage("Module *.TestModule not found");
        }

        private class ParentTestModule
        {
        }
    }
}
