using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Modules;

namespace Kolyhalov.FatNetLib.Core.Recorders
{
    public interface IModulesRecorder
    {
        public IModulesRecorder Register(IModule module);

        public IModulesRecorder Register(IEnumerable<IModule> modules);

        public IModulesRecorder Ignore<T>() where T : IModule;

        public IModulesRecorder Replace<T>(IModule module) where T : IModule;

        public void Setup(ModuleContext moduleContext);
    }
}
