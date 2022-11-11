using System.Collections.Generic;

namespace Kolyhalov.FatNetLib.Modules
{
    public interface IModule
    {
        public void Setup(ModuleContext moduleContext);

        public IList<IModule>? ChildModules { get; }
    }
}
