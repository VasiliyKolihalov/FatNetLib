using System;
using System.Collections.Generic;
using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Tests.Utils;
using NUnit.Framework;
using static System.Text.Encoding;

namespace Kolyhalov.FatNetLib.Tests;

public class PackageTests
{
    [Test, AutoData]
    public void SetField_SomeValue_ValueInFields(object value)
    {
        // Arrange
        var package = new Package();

        // Act
        package.SetField("CustomField", value);

        // Assert
        package.Fields["CustomField"].Should().Be(value);
    }

    [Test, AutoData]
    public void GetField_SomeValueInFields_ReturnValue(object value)
    {
        // Arrange
        var package = new Package { ["CustomField"] = value };

        // Act
        var returnedValue = package.GetField<object>("CustomField");

        // Assert
        returnedValue.Should().Be(value);
    }

    [Test, AutoData]
    public void GetField_ObjectWasNotSet_ReturnNull(object value)
    {
        // Assert
        new Package().GetField<object>("CustomField").Should().BeNull();
    }

    [Test, AutoData]
    public void GetField_StructWasNotSet_ReturnEmptyStruct(object value)
    {
        // Assert
        new Package().GetField<Guid>("CustomField").Should().BeEmpty();
    }

    [Test, AutoData]
    public void GetField_PrimitiveWasNotSet_ReturnDefault(object value)
    {
        // Assert
        new Package().GetField<bool>("CustomField").Should().BeFalse();
    }

    [Test, AutoData]
    public void Route_SetAndGetSomeValue_ReturnValue(string route)
    {
        // Act
        var package = new Package { Route = new Route(route) };

        // Assert
        package.Route.Should().Be(new Route(route));
    }

    [Test, AutoData]
    public void Body_SetAndGetSomeValue_ReturnValue(IDictionary<string, object> body)
    {
        // Act
        var package = new Package { Body = body };

        // Assert
        package.Body.Should().BeSameAs(body);
    }

    [Test, AutoData]
    public void GetBodyAs_SomeValue_ReturnValue(IDictionary<string, object> body)
    {
        // Act
        var package = new Package { Body = body };

        // Assert
        package.GetBodyAs<IDictionary<string, object>>().Should().BeSameAs(body);
    }

    [Test, AutoData]
    public void ExchangeId_SetAndGetSomeValue_ReturnValue(Guid exchangeId)
    {
        // Act
        var package = new Package { ExchangeId = exchangeId };

        // Assert
        package.ExchangeId.Should().Be(exchangeId);
    }

    [Test]
    public void IsResponse_SetAndGetSomeValue_ReturnValue()
    {
        // Act
        var package = new Package { IsResponse = true };

        // Assert
        package.IsResponse.Should().BeTrue();
    }

    [Test, AutoData]
    public void Indexer_SetAndGetSomeValue_ReturnValue(object value)
    {
        // Act
        var package = new Package { ["CustomField"] = value };

        // Assert
        package["CustomField"].Should().Be(value);
    }

    [Test, AutoData]
    public void SetNonSendingField_SomeValue_SetValue(object value)
    {
        // Arrange
        var package = new Package();

        // Act
        package.SetNonSendingField("CustomNonSendingField", value);

        // Assert
        package.NonSendingFields["CustomNonSendingField"].Should().Be(value);
    }

    [Test, AutoData]
    public void GetNonSendingField_SomeValueIsPresent_ReturnValue(object value)
    {
        // Arrange
        var package = new Package { NonSendingFields = { ["CustomNonSendingField"] = value } };

        // Act
        var returnedValue = package.GetNonSendingField<object>("CustomNonSendingField");

        // Assert
        returnedValue.Should().Be(value);
    }

    [Test, AutoData]
    public void GetNonSendingField_ObjectWasNotSet_ReturnNull(object value)
    {
        // Assert
        new Package().GetNonSendingField<object>("CustomNonSendingField").Should().BeNull();
    }

    [Test, AutoData]
    public void GetNonSendingField_StructWasNotSet_ReturnEmptyStruct(object value)
    {
        // Assert
        new Package().GetNonSendingField<Guid>("CustomNonSendingField").Should().BeEmpty();
    }

    [Test, AutoData]
    public void GetNonSendingField_PrimitiveWasNotSet_ReturnDefault(object value)
    {
        // Assert
        new Package().GetNonSendingField<bool>("CustomNonSendingField").Should().BeFalse();
    }

    [Test]
    public void Serialized_SetAndGetSomeValue_ReturnValue()
    {
        // Act
        var package = new Package { Serialized = UTF8.GetBytes("serialized-package") };

        // Assert
        package.Serialized.Should().BeEquivalentToUtf8("serialized-package");
    }

    [Test, AutoData]
    public void Schema_SetAndGetSomeValue_ReturnValue(PackageSchema packageSchema)
    {
        // Act
        var package = new Package { Schema = packageSchema };

        // Assert
        package.Schema.Should().BeSameAs(packageSchema);
    }

    [Test, AutoData]
    public void Context_SetAndGetSomeValue_ReturnValue(DependencyContext context)
    {
        // Act
        var package = new Package { Context = context };

        // Assert
        package.Context.Should().Be(context);
    }

    [Test, AutoData]
    public void FromPeerId_SetAndGetSomeValue_ReturnValue(int peerId)
    {
        // Act
        var package = new Package { FromPeerId = peerId };

        // Assert
        package.FromPeerId.Should().Be(peerId);
    }

    [Test, AutoData]
    public void ToPeerId_SetAndGetSomeValue_ReturnValue(int peerId)
    {
        // Act
        var package = new Package { ToPeerId = peerId };

        // Assert
        package.ToPeerId.Should().Be(peerId);
    }

    [Test, AutoData]
    public void Reliability_SetAndGetSomeValue_ReturnValue(Reliability reliability)
    {
        // Act
        var package = new Package { Reliability = reliability };

        // Assert
        package.Reliability.Should().Be(reliability);
    }
}
