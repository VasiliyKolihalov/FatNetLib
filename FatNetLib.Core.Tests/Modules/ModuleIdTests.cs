using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Modules;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests.Modules;

public class ModuleIdTests
{
    [Test]
    public void OperatorSlash_ConcatWithType_ReturnConcatenatedModuleId()
    {
        // Act
        ModuleId concatModuleId = new ModuleId(typeof(ModuleA)) / typeof(ModuleB);

        // Assert
        concatModuleId.Should().BeEquivalentTo(new ModuleId(typeof(ModuleA), typeof(ModuleB)));
    }

    [Test]
    public void OperatorSlash_ConcatWithModuleId_ReturnConcatenatedModuleId()
    {
        // Arrange
        var moduleId1 = new ModuleId(typeof(ModuleA));
        var moduleId2 = new ModuleId(typeof(ModuleB), typeof(ModuleC));

        // Act
        ModuleId concatModuleId = moduleId1 / moduleId2;

        // Assert
        concatModuleId.Should().BeEquivalentTo(
            new ModuleId(typeof(ModuleA), typeof(ModuleB), typeof(ModuleC)));
    }

    [Test]
    public void Equals_TwoSimilarModuleIds_ReturnTrue()
    {
        // Arrange
        var firstModuleId = new ModuleId(typeof(ModuleA), typeof(ModuleB));
        var secondModuleId = new ModuleId(typeof(ModuleA), typeof(ModuleB));

        // Assert
        firstModuleId.Equals(secondModuleId).Should().BeTrue();
    }

    private class ModuleA
    {
    }

    private class ModuleB
    {
    }

    private class ModuleC
    {
    }
}
