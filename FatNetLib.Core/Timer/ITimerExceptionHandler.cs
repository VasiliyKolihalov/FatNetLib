using System;

namespace Kolyhalov.FatNetLib.Core.Timer
{
    public interface ITimerExceptionHandler
    {
        public void Handle(Exception exception);
    }
}
