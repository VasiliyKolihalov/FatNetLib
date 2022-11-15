using System;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Subscribers;

namespace Kolyhalov.FatNetLib.Core.Timers
{
    public interface ITimer
    {
        public Frequency Frequency { get; set; }

        public void Start(Action action, ITimerExceptionHandler exceptionHandler);

        public void Stop();
    }
}
