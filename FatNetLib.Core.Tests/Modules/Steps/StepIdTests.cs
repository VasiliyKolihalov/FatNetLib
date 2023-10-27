using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Modules;
using Kolyhalov.FatNetLib.Core.Modules.Steps;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests.Modules.Steps;

public class StepIdTests
{
    [Test]
    public void Equals_TwoSimilarStepIds_ReturnTrue()
    {
        // Arrange
        var firstStepId = new StepId(new ModuleId(typeof(TestModule)), typeof(TestStep), "test-qualifier");
        var secondStepId = new StepId(new ModuleId(typeof(TestModule)), typeof(TestStep), "test-qualifier");

        // Assert
        firstStepId.Equals(secondStepId).Should().BeTrue();
    }

    private class TestModule
    {
    }

    private class TestStep
    {
    }
}
