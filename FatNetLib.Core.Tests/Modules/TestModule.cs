using Kolyhalov.FatNetLib.Core.Modules;
using NUnit.Framework;

namespace Kolyhalov.FatNetLib.Core.Tests.Modules
{
    [TestFixture]
    public class TestModule : IModule
    {
        public void Setup(IModuleContext moduleContext)
        {
            // no actions required
        }
    }
}
