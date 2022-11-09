namespace Kolyhalov.FatNetLib.Timer;

public interface ITimerExceptionHandler
{
    public void Handle(Exception exception);
}
