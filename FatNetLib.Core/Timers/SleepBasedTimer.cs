using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Microtypes;
using Kolyhalov.FatNetLib.Core.Subscribers;
using static System.Runtime.CompilerServices.MethodImplOptions;

namespace Kolyhalov.FatNetLib.Core.Timers
{
    public class SleepBasedTimer : ITimer
    {
        private bool _isActive;

        public SleepBasedTimer(Frequency frequency)
        {
            Frequency = frequency;
        }

        public Frequency Frequency { get; set; }

        [MethodImpl(Synchronized)]
        public void Start(Action action, ITimerExceptionHandler exceptionHandler)
        {
            _isActive = true;
            var periodStopwatch = new Stopwatch();
            periodStopwatch.Start();
            while (_isActive)
            {
                try
                {
                    Frequency currentFrequency = Frequency;
                    action.Invoke();

                    TimeSpan remainingSleep = currentFrequency.Period - periodStopwatch.Elapsed;
                    if (remainingSleep <= TimeSpan.Zero)
                    {
                        throw new ThrottlingFatNetLibException(currentFrequency.Period, periodStopwatch.Elapsed);
                    }

                    Thread.Sleep(remainingSleep);
                    periodStopwatch.Restart();
                }
                catch (Exception exception)
                {
                    periodStopwatch.Restart();
                    exceptionHandler.Handle(exception);
                }
            }

            periodStopwatch.Reset();
        }

        public void Stop()
        {
            _isActive = false;
        }
    }
}
