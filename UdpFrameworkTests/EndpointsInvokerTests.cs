using System;
using System.Collections.Generic;
using System.Linq;
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
    public void InvokeEndpoint_BuilderStyleReceiver_Invoke()
    {
        // Arrange
        var endpoint = new Endpoint(It.IsAny<string>(), EndpointType.Receiver, It.IsAny<DeliveryMethod>());
        Mock<Pacifier> mock = new Mock<Pacifier>();
        mock.Setup(x => x.Do());
        ReceiverDelegate receiverDelegate = _ => { mock.Object.Do(); };
        var localEndpoint = new LocalEndpoint(endpoint, controller: null, receiverDelegate.Method);
        
        // Act
        void Action() => _endpointsInvoker!.InvokeEndpoint(localEndpoint, package: null!);

        // Assert
        Assert.Throws<SuccessException>(Action);
        mock.Verify(x => x.Do(), Times.Once);

    }
    
    

    [Test]
    public void InvokeEndpoint_ControllerStyleReceiver_Invoke()
    {
        // Arrange
        LocalEndpoint endpoint = CreateReceiverEndpointFromController(new ControllerWithReceiverWithParameter());
        var package = new Package
        {
            Body = new Dictionary<string, object> {["Parameter"] = JsonConvert.SerializeObject(new Parameter())}
        };

        // Act
        void Action() => _endpointsInvoker!.InvokeEndpoint(endpoint, package);

        // Assert
        Assert.Throws<TargetInvocationException>(Action);
    }

    [Test]
    public void InvokeEndpoint_PackageWithoutArgument_Throw()
    {
        // Arrange
        LocalEndpoint endpoint = CreateReceiverEndpointFromController(new ControllerWithReceiverWithParameter());

        // Act
        void Action() => _endpointsInvoker!.InvokeEndpoint(endpoint, new Package());

        // Assert
        Assert.That(Action, Throws.TypeOf<UdpFrameworkException>()
            .With.Message.Contains("There is no required field: Parameter in the package"));
    }

    private static LocalEndpoint CreateReceiverEndpointFromController(IController controller)
    {
        Type type = controller.GetType();

        MethodInfo methodInfo = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
            .First();
        var endpoint = new Endpoint(It.IsAny<string>(), EndpointType.Receiver, It.IsAny<DeliveryMethod>());
        var localEndpoint = new LocalEndpoint(endpoint, controller, methodInfo);

        return localEndpoint;
    }

    #region resource classes

    private class ControllerWithReceiverWithParameter : IController
    {
        public void Receiver(Parameter _)
        {
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