using System.Net;
using System.Net.Sockets;
using Kolyhalov.FatNetLib.Core.Microtypes;

namespace Kolyhalov.FatNetLib.IntegrationTests;

public static class TestUtils
{
    public static Port FindFreeTcpPort()
    {
        var tcpListener = new TcpListener(IPAddress.Loopback, port: 0);
        tcpListener.Start();
        int port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
        tcpListener.Stop();
        return new Port(port);
    }
}
