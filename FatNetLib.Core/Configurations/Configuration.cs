using System;
using Kolyhalov.FatNetLib.Core.Microtypes;

namespace Kolyhalov.FatNetLib.Core.Configurations
{
    public abstract class Configuration
    {
        public Port? Port { get; set; }

        public Frequency? Framerate { get; set; }

        public TimeSpan? ExchangeTimeout { get; set; }

        public PackageSchema? PackageSchemaPatch { get; set; }

        public virtual void Patch(Configuration patch)
        {
            if (patch.Port != null)
                Port = patch.Port;

            if (patch.Framerate != null)
                Framerate = patch.Framerate;

            if (patch.ExchangeTimeout != null)
                ExchangeTimeout = patch.ExchangeTimeout;

            if (patch.PackageSchemaPatch == null) return;
            if (PackageSchemaPatch == null)
            {
                PackageSchemaPatch = patch.PackageSchemaPatch;
                return;
            }

            PackageSchemaPatch.Patch(patch.PackageSchemaPatch);
        }
    }
}
