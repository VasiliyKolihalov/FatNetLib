namespace Framework;

public class UdpFramework
{
    public UdpFrameworkClient UdpFrameworkClient { get; set; }
    
    public void Run() {}
    
    public UdpFrameworkBuilder AddController(IController controller)
    {
        return new UdpFrameworkBuilder();
    }
    
}