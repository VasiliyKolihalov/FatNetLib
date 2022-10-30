namespace Kolyhalov.FatNetLib.Configurations;

public class ClientConfiguration : Configuration
{
    public string? Address { get; set; }

    public override void Patch(Configuration other)
    {
        base.Patch(other);
        if (other is ClientConfiguration clientConfiguration && clientConfiguration.Address != null)
            Address = clientConfiguration.Address;
    }
}