using System;

namespace Kolyhalov.FatNetLib.Core.Modules.Steps
{
    public class BeginModuleStep : IModuleStep
    {
        public BeginModuleStep(Type moduleType, Type parentModuleType)
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
            return new BeginModuleStep((Type)newId.InModuleId, newId.ParentModuleType);
        }
    }
}
