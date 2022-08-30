namespace Kolyhalov.UdpFramework;

public interface IMonitor
{
    public WaitingResult Wait(object monitorObject, TimeSpan timeout);
    
    public void Pulse(object monitorObject);
}