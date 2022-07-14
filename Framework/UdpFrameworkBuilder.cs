using LiteNetLib;

namespace Framework;

public class UdpFrameworkBuilder
{

    public UdpFramework BuildAndRun()
    {
        return new UdpFramework();
        
    }

    public UdpFrameworkBuilder RouteSender(string path, DeliveryMethod deliveryMethod, Func<Package, Package> endpoint)
    {
        return new UdpFrameworkBuilder();
    }
    
    public UdpFramework Build()
    {
        return new UdpFramework();
        
    }
}