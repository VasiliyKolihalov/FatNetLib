using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Storages;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests.Storages
{
    public class DependencyContextTests
    {
        private DependencyContext _context = null!;

        [SetUp]
        public void SetUp()
        {
            _context = new DependencyContext();
        }

        [Test, AutoData]
        public void Put_ByStringId_ReturnDependency(object dependency)
        {
            // Act
            _context.Put("some-id", dependency);

            // Assert
            _context.Get<object>("some-id").Should().Be(dependency);
        }

        [Test, AutoData]
        public void Put_ReplacingOld_ReturnDependency(object oldDependency, object newDependency)
        {
            // Arrange
            _context.Put("some-id", oldDependency);

            // Act
            _context.Put("some-id", newDependency);

            // Assert
            _context.Get<object>("some-id").Should().Be(newDependency);
        }

        [Test, AutoData]
        public void Put_ByType_ReturnDependency(object dependency)
        {
            // Act
            _context.Put(dependency);

            // Assert
            _context.Get<object>().Should().Be(dependency);
        }
    }
}
