using Kolyhalov.FatNetLib.Configurations;
using Kolyhalov.FatNetLib.Microtypes;
using Kolyhalov.FatNetLib.Wrappers;
using Moq;
using NUnit.Framework;
using static Moq.Times;

namespace Kolyhalov.FatNetLib;

public class ServerConnectionStarterTests
{
    private ServerConnectionStarter _starter = null!;
    private Mock<INetManager> _netManager = null!;

    [SetUp]
    public void SetUp()
    {
        _netManager = new Mock<INetManager>();

        var configuration = new ClientConfiguration("12.34.56.78",
            new Port(123),
            framerate: null!,
            exchangeTimeout: null!);

        _starter = new ServerConnectionStarter(_netManager.Object, configuration);
    }

    [Test]
    public void StartConnection_CorrectCase_CallNetManager()
    {
        // Act
        _starter.StartConnection();

        // Assert
        _netManager.Verify(_ => _.Start(123), Once);
        _netManager.VerifyNoOtherCalls();
    }
}