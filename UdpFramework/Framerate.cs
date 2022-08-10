namespace Kolyhalov.UdpFramework;

public class Framerate
{
    public int ServerFramerate { get; }

    public Framerate(int framerate)
    {
        if (framerate < 0)
            throw new UdpFrameworkException("Framerate cannot be below zero");

        ServerFramerate = framerate;
    }
}