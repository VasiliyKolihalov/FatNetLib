using System;

namespace Kolyhalov.FatNetLib.Wrappers
{
    // Object-oriented style wrapper of the default System.Threading.Monitor
    public class Monitor : IMonitor
    {
        public WaitingResult Wait(object monitorObject, TimeSpan timeout)
        {
            bool pulseReceived = System.Threading.Monitor.Wait(monitorObject, timeout);
            return pulseReceived
                ? WaitingResult.PulseReceived
                : WaitingResult.InterruptedByTimeout;
        }

        public void Pulse(object monitorObject)
        {
            System.Threading.Monitor.Pulse(monitorObject);
        }
    }

    public enum WaitingResult
    {
        PulseReceived,
        InterruptedByTimeout
    }
}
