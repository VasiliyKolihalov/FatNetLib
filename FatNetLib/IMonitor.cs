namespace Kolyhalov.FatNetLib;

public interface IMonitor
{
    public WaitingResult Wait(object monitorObject, TimeSpan timeout);
    
    public void Pulse(object monitorObject);
}