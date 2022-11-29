using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Loggers;
using Kolyhalov.FatNetLib.Core.Modules.Defaults;
using Kolyhalov.FatNetLib.Core.Modules.Steps;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests.Modules.Steps
{
    public class StepIdTests
    {
        [Test]
        public void Equals_TwoSimilarStepIds_ReturnTrue()
        {
            // Arrange
            var stepId1 = new StepId(
                parentModuleType: typeof(DefaultCommonModule),
                stepType: typeof(PutDependencyStep),
                typeof(ILogger));

            var stepId2 = new StepId(
                parentModuleType: typeof(DefaultCommonModule),
                stepType: typeof(PutDependencyStep),
                typeof(ILogger));

            // Act
            bool equality = stepId1.Equals(stepId2);

            // Assert
            equality.Should().BeTrue();
        }
    }
}
