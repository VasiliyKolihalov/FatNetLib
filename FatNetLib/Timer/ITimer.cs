using System;
using Kolyhalov.FatNetLib.Microtypes;

namespace Kolyhalov.FatNetLib.Timer
{
    public interface ITimer
    {
        public Frequency Frequency { get; set; }

        public void Start(Action action, ITimerExceptionHandler exceptionHandler);

        public void Stop();
    }
}
