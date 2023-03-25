using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoFixture.NUnit3;
using FluentAssertions;
using Kolyhalov.FatNetLib.Core.Attributes;
using Kolyhalov.FatNetLib.Core.Components;
using Kolyhalov.FatNetLib.Core.Configurations;
using Kolyhalov.FatNetLib.Core.Controllers;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Models;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests.Components;

public class ControllerArgumentsResolverTests
{
    private readonly ControllerArgumentsExtractor _extractor = new();

    [Test, AutoData]
    public void Test(EndpointType endpointType, Reliability reliability)
    {
        // Arrange
        var controller = new TestController();
        MethodInfo method = controller.GetType().GetMethod("TestEndpoint")!;
        var endpoint = new LocalEndpoint(
            new Endpoint(
                new Route("test"),
                endpointType,
                reliability,
                requestSchemaPatch: new PackageSchema(),
                responseSchemaPatch: new PackageSchema()),
            action: CreateAction(method, controller));
        var package = new Package
        {
            Body = "test-body",
            ["TestField"] = 42
        };

        // Act
        object?[] arguments = _extractor.ExtractFromPackage(package, endpoint);

        // Assert
        arguments.Should().Equal(package, "test-body", 42, null);
    }

    private static Delegate CreateAction(MethodInfo methodInfo, IController controller)
    {
        IEnumerable<Type> paramTypes = methodInfo.GetParameters().Select(parameter => parameter.ParameterType);
        Type delegateType = Expression.GetDelegateType(paramTypes.Append(methodInfo.ReturnType).ToArray());
        return methodInfo.CreateDelegate(delegateType, controller);
    }

    private class TestController : IController
    {
        public void TestEndpoint(
            Package package,
            [Body] string body,
            [FromPackage("TestField")] int testValue,
            [FromPackage("UnknownField")] int unknownValue)
        {
            throw new InvalidOperationException("this test method shouldn't be called");
        }
    }
}
