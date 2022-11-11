namespace Kolyhalov.FatNetLib.Configurations
{
    public class ClientConfiguration : Configuration
    {
        public string? Address { get; set; }

        public override void Patch(Configuration patch)
        {
            if (!(patch is ClientConfiguration clientConfiguration))
                throw new FatNetLibException(
                    "Failed to patch. Wrong type of configuration. Should be ClientConfiguration");

            base.Patch(patch);

            if (clientConfiguration.Address != null)
                Address = clientConfiguration.Address;
        }
    }
}
