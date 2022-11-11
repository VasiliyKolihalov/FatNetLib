using System;

namespace Kolyhalov.FatNetLib
{
    public class FatNetLibException : Exception
    {
        public FatNetLibException(string message) : base(message)
        {
        }

        public FatNetLibException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
