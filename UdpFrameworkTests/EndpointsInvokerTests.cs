using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Kolyhalov.UdpFramework;
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
        var endpoint = new Endpoint(It.IsAny<string>(), EndpointType.Receiver, It.IsAny<DeliveryMethod>());
        var pacifierMock = new Mock<Pacifier>();
        pacifierMock.Setup(x => x.Do());
        void ReceiverDelegate(Package _) => pacifierMock.Object.Do();
        var localEndpoint = new LocalEndpoint(endpoint, (ReceiverDelegate) ReceiverDelegate);

        // Act
        Package? result = _endpointsInvoker!.InvokeEndpoint(localEndpoint, new Package());

        // Assert
        Assert.Null(result);
        pacifierMock.Verify(x => x.Do(), Times.Once);
    }

    [Test]
    public void InvokeEndpoint_BuilderStyleExchanger_InvokeAndReturn()
    {
        // Arrange
        var endpoint = new Endpoint(It.IsAny<string>(), EndpointType.Exchanger, It.IsAny<DeliveryMethod>());
        var pacifierMock = new Mock<Pacifier>();
        pacifierMock.Setup(x => x.Do());

        Package ReceiverDelegate(Package _)
        {
            pacifierMock.Object.Do();
            return new Package();
        }

        var localEndpoint = new LocalEndpoint(endpoint, (ExchangerDelegate) ReceiverDelegate);

        // Act
        Package? result = _endpointsInvoker!.InvokeEndpoint(localEndpoint, new Package());

        // Assert
        Assert.NotNull(result);
        pacifierMock.Verify(x => x.Do(), Times.Once);
    }


    [Test]
    public void InvokeEndpoint_ControllerStyleReceiverWithParameters_InvokeAndReturnNull()
    {
        // Arrange
        var pacifierMock = new Mock<Pacifier>();
        pacifierMock.Setup(x => x.Do());
        LocalEndpoint endpoint =
            CreateEndpointFromController(new ControllerWithReceiverWithParameter(pacifierMock.Object),
                EndpointType.Receiver);
        var package = new Package
        {
            Body = new Dictionary<string, object> {["Parameter"] = JsonConvert.SerializeObject(new Parameter())}
        };

        // Act
        Package? result = _endpointsInvoker!.InvokeEndpoint(endpoint, package);

        // Assert
        Assert.Null(result);
        pacifierMock.Verify(x => x.Do(), Times.Once);
    }

    [Test]
    public void InvokeEndpoint_ControllerStyleExchangerWithParameters_InvokeAndReturn()
    {
        // Arrange
        var pacifierMock = new Mock<Pacifier>();
        pacifierMock.Setup(x => x.Do());
        LocalEndpoint endpoint = CreateEndpointFromController(
            new ControllerWithExchangerWithParameter(pacifierMock.Object),
            EndpointType.Exchanger);
        var package = new Package
        {
            Body = new Dictionary<string, object> {["Parameter"] = JsonConvert.SerializeObject(new Parameter())}
        };

        // Act
        Package? result = _endpointsInvoker!.InvokeEndpoint(endpoint, package);

        // Assert
        Assert.NotNull(result);
        pacifierMock.Verify(x => x.Do(), Times.Once);
    }

    [Test]
    public void InvokeEndpoint_NullBodyPackage_Throw()
    {
        // Arrange
        LocalEndpoint endpoint =
            CreateEndpointFromController(new ControllerWithReceiverWithParameter(Mock.Of<Pacifier>()),
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
            new ControllerWithReceiverWithParameter(Mock.Of<Pacifier>()), EndpointType.Receiver);

        // Act
        void Action() =>
            _endpointsInvoker!.InvokeEndpoint(endpoint, new Package {Body = new Dictionary<string, object>()});

        // Assert
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>()
            .With.Message.EqualTo("There is no required field: Parameter in the package"));
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

        var endpoint = new Endpoint(It.IsAny<string>(), endpointType, It.IsAny<DeliveryMethod>());
        var localEndpoint = new LocalEndpoint(endpoint, methodDelegate);

        return localEndpoint;
    }

    #region resource classes

    private class ControllerWithReceiverWithParameter : IController
    {
        private readonly Pacifier _pacifier;

        public ControllerWithReceiverWithParameter(Pacifier pacifier)
        {
            _pacifier = pacifier;
        }

        public void Receiver(Parameter _)
        {
            _pacifier.Do();
        }
    }

    private class ControllerWithExchangerWithParameter : IController
    {
        private readonly Pacifier _pacifier;

        public ControllerWithExchangerWithParameter(Pacifier pacifier)
        {
            _pacifier = pacifier;
        }

        public Package Exchanger(Parameter _)
        {
            _pacifier.Do();
            return new Package();
        }
    }

    private class Parameter
    {
    }

    public abstract class Pacifier
    {
        public abstract void Do();
    }

    #endregion
}