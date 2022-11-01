using System;
using FluentAssertions;
using NUnit.Framework;
using static System.Text.Encoding;

namespace Kolyhalov.FatNetLib.Utils;

public class AssertUtilsTests
{
    [Test]
    public void BeEquivalentToUtf8_EqualValues_Pass()
    {
        // Assert
        UTF8.GetBytes("some-value").Should().BeEquivalentToUtf8("some-value")
            .And.NotBeNull();
    }

    [Test]
    public void BeEquivalentToUtf8_TwoDifferentValues_Throw()
    {
        // Act
        Action act = () => UTF8.GetBytes("some-value").Should().BeEquivalentToUtf8("another-value");

        // Assert
        act.Should().Throw<AssertionException>()
            .WithMessage("Expected root to be a collection with 13 item(s), but *");
    }

    [Test]
    public void BeEquivalentToUtf8_AnotherEncoding_Throw()
    {
        // Act
        Action act = () => Latin1.GetBytes("some-value").Should().BeEquivalentToUtf8("another-value");

        // Assert
        act.Should().Throw<AssertionException>()
            .WithMessage("Expected root to be a collection with 13 item(s), but *");
    }
}