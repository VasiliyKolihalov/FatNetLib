using System;
using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.Wrappers;
using Moq;
using NUnit.Framework;
using static Moq.Times;

namespace Kolyhalov.FatNetLib;

public class ClientConnectionStarterTests
{
    private ClientConnectionStarter _starter = null!;
    private Mock<INetManager> _netManager = null!;

    [SetUp]
    public void SetUp()
    {
        _netManager = new Mock<INetManager>();
        
        var configuration = new ClientConfiguration("12.34.56.78",
            new Port(123), 
            framerate: null!,
            exchangeTimeout: null!);
        
        var protocolVersionProvider = new Mock<IProtocolVersionProvider>();
        protocolVersionProvider.Setup(_ => _.Get())
            .Returns("test-protocol");
        
        _starter = new ClientConnectionStarter(_netManager.Object, configuration, protocolVersionProvider.Object);
    }
    
    [Test]
    public void StartConnection_CorrectCase_CallNetManager()
    {
        // Act
        _starter.StartConnection();
        
        // Assert
        _netManager.Verify(_ => _.Start(), Once);
        _netManager.Verify(_ => _.Connect("12.34.56.78", 123, "test-protocol"), Once);
        _netManager.VerifyNoOtherCalls();
    }
}