namespace Kolyhalov.UdpFramework;

[Serializable]
public class UdpFrameworkException : Exception
{
    public UdpFrameworkException(string message) : base(message)
    {
    }

    public UdpFrameworkException(string message, Exception innerException) : base(message, innerException)
    {
    }
}