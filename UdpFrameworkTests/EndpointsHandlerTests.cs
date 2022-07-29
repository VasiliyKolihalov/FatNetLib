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

public class EndpointsHandlerTests
{
    [Test]
    public void HandelEndpointShouldInvokeBuilderStyleReceiver()
    {
        Endpoint endpoint = new Endpoint(It.IsAny<string>(), EndpointType.Receiver, It.IsAny<DeliveryMethod>());
        ReceiverDelegate receiverDelegate = _ => { };
        LocalEndpoint localEndpoint = new LocalEndpoint(endpoint, null, receiverDelegate.Method);
        EndpointsHandler endpointsHandler = new EndpointsHandler();

        TestDelegate testDelegate = () => endpointsHandler.HandleEndpoint(localEndpoint, null!);

        Assert.DoesNotThrow(testDelegate);
    }

    [Test]
    public void HandelEndpointShouldInvokeControllerStyleReceiver()
    {
        LocalEndpoint endpoint = GetReceiverEndpointFromController(new ControllerWithReceiverWithParameter());
        Package package = new Package
            {Body = new Dictionary<string, object> {["Parameter"] = JsonConvert.SerializeObject(new Parameter())}};
        EndpointsHandler endpointsHandler = new EndpointsHandler();

        TestDelegate testDelegate = () => endpointsHandler.HandleEndpoint(endpoint, package);

        Assert.DoesNotThrow(testDelegate);
    }

    [Test]
    public void HandelEndpointShouldThrowBecauseInPackageNoArgument()
    {
        LocalEndpoint endpoint = GetReceiverEndpointFromController(new ControllerWithReceiverWithParameter());

        EndpointsHandler endpointsHandler = new EndpointsHandler();

        TestDelegate testDelegate = () => endpointsHandler.HandleEndpoint(endpoint, null!);

        Assert.Throws<UdpFrameworkException>(testDelegate);
    }

    private static LocalEndpoint GetReceiverEndpointFromController(IController controller)
    {
        Type type = controller.GetType();

        MethodInfo methodInfo = type.GetMethods(UdpFramework.EndpointSearch).First();
        Endpoint endpoint = new Endpoint(It.IsAny<string>(), EndpointType.Receiver, It.IsAny<DeliveryMethod>());
        LocalEndpoint localEndpoint = new LocalEndpoint(endpoint, controller, methodInfo);

        return localEndpoint;
    }

    #region resource classes

    private class ControllerWithReceiverWithParameter : IController
    {
        public void Receiver(Parameter parameter) { }
    }

    private class Parameter { }
    
    #endregion
}