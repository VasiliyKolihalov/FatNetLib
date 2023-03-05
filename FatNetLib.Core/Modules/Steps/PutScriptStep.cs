using System;
using Kolyhalov.FatNetLib.Core.Storages;

namespace Kolyhalov.FatNetLib.Core.Modules.Steps
{
    public class PutScriptStep : IStep
    {
        private readonly Action<IDependencyContext> _script;
        private readonly IDependencyContext _dependencyContext;

        public PutScriptStep(string scriptName, Action<IDependencyContext> script, IDependencyContext dependencyContext)
        {
            Qualifier = scriptName;
            _script = script;
            _dependencyContext = dependencyContext;
        }

        public object Qualifier { get; }

        public void Run()
        {
            _script.Invoke(_dependencyContext);
        }
    }
}
