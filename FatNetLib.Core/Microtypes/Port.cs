using Kolyhalov.FatNetLib.Core.Exceptions;

namespace Kolyhalov.FatNetLib.Core.Microtypes
{
    public class Port
    {
        public int Value { get; }

        private const int MinValidPort = 0;
        private const int MaxValidPort = 65535;

        public Port(int value)
        {
            if (value < MinValidPort || value > MaxValidPort)
                throw new FatNetLibException($"Invalid port. Valid range is {MinValidPort} to {MaxValidPort}");

            Value = value;
        }
    }
}
