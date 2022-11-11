using System;
using Kolyhalov.FatNetLib.Core.Microtypes;

namespace Kolyhalov.FatNetLib.Core.Timer
{
    public interface ITimer
    {
        public Frequency Frequency { get; set; }

        public void Start(Action action, ITimerExceptionHandler exceptionHandler);

        public void Stop();
    }
}
