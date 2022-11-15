using System;

namespace Kolyhalov.FatNetLib.Core.Subscribers
{
    public interface ITimerExceptionHandler
    {
        public void Handle(Exception exception);
    }
}
