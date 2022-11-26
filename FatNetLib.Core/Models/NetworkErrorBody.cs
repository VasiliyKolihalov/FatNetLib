using System.Net;
using System.Net.Sockets;

namespace Kolyhalov.FatNetLib.Core.Models
{
    public class NetworkErrorBody
    {
        public IPEndPoint IPEndPoint { get; set; } = null!;

        public SocketError SocketError { get; set; }
    }
}
