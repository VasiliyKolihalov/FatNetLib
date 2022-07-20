using System.Reflection;
using LiteNetLib.Utils;

namespace UdpFramework;

public class Package
{
    public string Route { get; set; }
    public Dictionary<string, object> Body { get; set; }
}