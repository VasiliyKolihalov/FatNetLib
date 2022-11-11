﻿using Kolyhalov.FatNetLib.Microtypes;

namespace Kolyhalov.FatNetLib.Configurations
{
    public class ServerConfiguration : Configuration
    {
        public Count? MaxPeers { get; set; }

        public override void Patch(Configuration patch)
        {
            if (!(patch is ServerConfiguration serverConfiguration))
                throw new FatNetLibException(
                    "Failed to patch. Wrong type of configuration. Should be ServerConfiguration");

            base.Patch(patch);

            if (serverConfiguration.MaxPeers != null)
                MaxPeers = serverConfiguration.MaxPeers;
        }
    }
}
