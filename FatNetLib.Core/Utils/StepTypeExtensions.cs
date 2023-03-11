using System;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Modules.Steps;

namespace Kolyhalov.FatNetLib.Core.Utils
{
    public static class StepTypeExtensions
    {
        public static Type ToClass(this StepType stepType)
        {
            return stepType switch
            {
                StepType.MoveStepAfter => typeof(MoveStepAfterStep),
                StepType.MoveStepBefore => typeof(MoveStepBeforeStep),
                StepType.PutController => typeof(PutControllerStep<>),
                StepType.PutDependency => typeof(PutDependencyStep),
                StepType.PutModule => typeof(PutModuleStep),
                StepType.PutScript => typeof(PutScriptStep),
                StepType.RemoveModule => typeof(RemoveModuleStep),
                StepType.RemoveStep => typeof(RemoveStepStep),
                StepType.ReplaceOldStep => typeof(ReplaceOldStepStep),
                _ => throw new FatNetLibException($"{stepType} is not supported")
            };
        }
    }
}
