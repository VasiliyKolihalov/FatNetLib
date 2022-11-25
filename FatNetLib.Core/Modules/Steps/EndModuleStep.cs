using System;

namespace Kolyhalov.FatNetLib.Core.Modules.Steps
{
    public class EndModuleStep : IModuleStep
    {
        public EndModuleStep(Type moduleType, Type parentModuleType)
        {
            Id = new StepId(parentModuleType, GetType(), moduleType);
        }

        public StepId Id { get; }

        public void Run()
        {
            // no actions required
        }

        public IModuleStep CopyWithNewId(StepId newId)
        {
            return new EndModuleStep((Type)newId.Qualifier, newId.ParentModuleType);
        }
    }
}
