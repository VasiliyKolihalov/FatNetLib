using System.Diagnostics;
using System.Reflection;
using Kolyhalov.FatNetLib.Core.Exceptions;

namespace Kolyhalov.FatNetLib.Core.Providers
{
    public class ProtocolVersionProvider : IProtocolVersionProvider
    {
        public string Get()
        {
            var assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersionInfo.ProductVersion
                   ?? throw new FatNetLibException("Project version is not specified");
        }
    }
}
