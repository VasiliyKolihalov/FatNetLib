using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Kolyhalov.UdpFramework;
using Kolyhalov.UdpFramework.Endpoints;
using LiteNetLib;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using static Kolyhalov.UdpFramework.UdpFramework;

namespace UdpFrameworkTests;

public class EndpointsInvokerTests
{
    private EndpointsInvoker? _endpointsInvoker;

    [SetUp]
    public void Setup()
    {
        _endpointsInvoker = new EndpointsInvoker();
    }

    [Test]
    public void InvokeEndpoint_BuilderStyleReceiver_InvokeAndReturnNull()
    {
        // Arrange
        var endpoint = new Endpoint("route", EndpointType.Receiver, It.IsAny<DeliveryMethod>());
        var receiverMock = new Mock<ReceiverDelegate>();
        var localEndpoint = new LocalEndpoint(endpoint, receiverMock.Object);
        var package = new Package();

        // Act
        Package? result = _endpointsInvoker!.InvokeEndpoint(localEndpoint, package);

        // Assert
        Assert.Null(result);
        receiverMock.Verify(_ => _.Invoke(package), Times.Once);
    }

    [Test]
    public void InvokeEndpoint_BuilderStyleExchanger_InvokeAndReturn()
    {
        // Arrange
        var endpoint = new Endpoint("route", EndpointType.Exchanger, It.IsAny<DeliveryMethod>());
        var exchangerDelegate = new Mock<ExchangerDelegate>();
        exchangerDelegate.Setup(_ => _.Invoke(It.IsAny<Package>())).Returns(new Package());
        var localEndpoint = new LocalEndpoint(endpoint, exchangerDelegate.Object);
        var package = new Package();

        // Act
        Package? result = _endpointsInvoker!.InvokeEndpoint(localEndpoint, package);

        // Assert
        Assert.NotNull(result);
        exchangerDelegate.Verify(_ => _.Invoke(package));
    }


    [Test]
    public void InvokeEndpoint_ControllerStyleReceiverWithParameters_InvokeAndReturnNull()
    {
        // Arrange
        var stubMock = new Mock<Stub>();
        LocalEndpoint endpoint = CreateEndpointFromController(
            new ControllerWithReceiverWithParameter(stubMock.Object),
            EndpointType.Receiver);
        var package = new Package
        {
            Body = new Dictionary<string, object> {["Parameter"] = JsonConvert.SerializeObject(new Parameter())}
        };

        // Act
        Package? result = _endpointsInvoker!.InvokeEndpoint(endpoint, package);

        // Assert
        Assert.Null(result);
        stubMock.Verify(stub => stub.Do(It.IsAny<Parameter>()), Times.Once);
    }

    [Test]
    public void InvokeEndpoint_ControllerStyleExchangerWithParameters_InvokeAndReturn()
    {
        // Arrange
        var stubMock = new Mock<Stub>();
        stubMock.Setup(stub => stub.Do());
        LocalEndpoint endpoint = CreateEndpointFromController(
            new ControllerWithExchangerWithParameter(stubMock.Object),
            EndpointType.Exchanger);
        var package = new Package
        {
            Body = new Dictionary<string, object> {["Parameter"] = JsonConvert.SerializeObject(new Parameter())}
        };

        // Act
        Package? result = _endpointsInvoker!.InvokeEndpoint(endpoint, package);

        // Assert
        Assert.NotNull(result);
        stubMock.Verify(stub => stub.Do(It.IsAny<Parameter>()), Times.Once);
    }

    [Test]
    public void InvokeEndpoint_NullBodyPackage_Throw()
    {
        // Arrange
        LocalEndpoint endpoint =
            CreateEndpointFromController(new ControllerWithReceiverWithParameter(Mock.Of<Stub>()),
                EndpointType.Receiver);

        // Act
        void Action() => _endpointsInvoker!.InvokeEndpoint(endpoint, new Package());

        // Assert
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>().With.Message.EqualTo("Package body is null"));
    }

    [Test]
    public void InvokeEndpoint_PackageWithoutArgument_Throw()
    {
        // Arrange
        LocalEndpoint endpoint = CreateEndpointFromController(
            new ControllerWithReceiverWithParameter(Mock.Of<Stub>()), EndpointType.Receiver);

        // Act
        void Action() =>
            _endpointsInvoker!.InvokeEndpoint(endpoint, new Package {Body = new Dictionary<string, object>()});

        // Assert
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>()
            .With.Message.EqualTo("There is no required field: parameter in the package"));
    }

    [Test]
    public void InvokeEndpoint_PackageFieldWithWrongType_Throw()
    {
        // Arrange
        LocalEndpoint endpoint = CreateEndpointFromController(
            new ControllerWithReceiverWithParameter(stub: null!), It.IsAny<EndpointType>());
        var package = new Package
        {
            Body = new Dictionary<string, object>
            {
                ["Parameter"] = JsonConvert.SerializeObject(new AnotherParameter())
            }
        };

        // Act
        void Action() => _endpointsInvoker!.InvokeEndpoint(endpoint, package);

        // Assert
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>()
            .With.Message.EqualTo("Failed to deserialize package field to parameter: parameter"));
    }

    [Test]
    public void InvokeEndpoint_EndpointThrow_Throw()
    {
        // Arrange
        LocalEndpoint endpoint = CreateEndpointFromController(
            new ControllerWithEndpointWhichThrowsException(), It.IsAny<EndpointType>());
        
        // Act
        void Action() => _endpointsInvoker!.InvokeEndpoint(endpoint, package:null!);
        
        // Assert
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>()
            .With.Message.EqualTo("Endpoint invocation failed"));
    }
    
    [Test]
    public void InvokeEndpoint_ExchangerReturnsNull_Throw()
    {
        // Arrange
        var endpoint = new Endpoint("route", EndpointType.Exchanger, It.IsAny<DeliveryMethod>());
        var exchangerDelegate = new Mock<ExchangerDelegate>();
        exchangerDelegate.Setup(_ => _.Invoke(It.IsAny<Package>())).Returns((Package) null!);
        var localEndpoint = new LocalEndpoint(endpoint, exchangerDelegate.Object);
        var package = new Package();

        // Act
        void Action() => _endpointsInvoker!.InvokeEndpoint(localEndpoint, package);

        // Assert
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>()
            .With.Message.EqualTo("Exchanger must return a package"));
    }

    private static LocalEndpoint CreateEndpointFromController(IController controller, EndpointType endpointType)
    {
        Type controllerType = controller.GetType();
        MethodInfo methodInfo = controllerType
            .GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
            .First();

        IEnumerable<Type> paramTypes = methodInfo.GetParameters().Select(parameter => parameter.ParameterType);

        Type delegateType = Expression.GetDelegateType(paramTypes.Append(methodInfo.ReturnType).ToArray());

        Delegate methodDelegate = methodInfo.CreateDelegate(delegateType, controller);

        var endpoint = new Endpoint("route", endpointType, It.IsAny<DeliveryMethod>());
        var localEndpoint = new LocalEndpoint(endpoint, methodDelegate);

        return localEndpoint;
    }

    #region resource classes

    private class ControllerWithReceiverWithParameter : IController
    {
        private readonly Stub _stub;

        public ControllerWithReceiverWithParameter(Stub stub)
        {
            _stub = stub;
        }

        public void Receiver(Parameter parameter)
        {
            _stub.Do(parameter);
        }
    }

    private class ControllerWithExchangerWithParameter : IController
    {
        private readonly Stub _stub;

        public ControllerWithExchangerWithParameter(Stub stub)
        {
            _stub = stub;
        }

        public Package Exchanger(Parameter parameter)
        {
            _stub.Do(parameter);
            return new Package();
        }
    }

    private class ControllerWithEndpointWhichThrowsException : IController
    {
        public void Endpoint()
        {
            throw new Exception("Endpoint exception");
        }
    }

    public class Parameter
    {
        public List<object>? Object { get; set; }
    }

    public class AnotherParameter
    {
        public object Object { get; set; } = new();
    }

    public abstract class Stub
    {
        public abstract void Do();
        public abstract void Do(object parameter);
    }

    #endregion
}