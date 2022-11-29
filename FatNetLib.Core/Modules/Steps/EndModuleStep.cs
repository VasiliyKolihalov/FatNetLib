using System;

namespace Kolyhalov.FatNetLib.Core.Modules.Steps
{
    public class EndModuleStep : IModuleStep
    {
        public EndModuleStep(ModuleId moduleId)
        {
            Id = new StepId(moduleId.ParentType, GetType(), moduleId.TargetType);
        }

        public StepId Id { get; }

        public void Run()
        {
            // no actions required
        }

        public IModuleStep CopyWithNewId(StepId newId)
        {
            return new EndModuleStep(new ModuleId((Type)newId.Qualifier, newId.ParentModuleType));
        }
    }
}
