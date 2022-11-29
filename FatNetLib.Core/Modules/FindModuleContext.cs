using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Modules.Steps;

namespace Kolyhalov.FatNetLib.Core.Modules
{
    public class FindModuleContext
    {
        private readonly ModuleId _moduleId;
        private readonly List<IModuleStep> _steps;
        private readonly IModuleContext _context;

        public FindModuleContext(ModuleId moduleId, List<IModuleStep> steps, IModuleContext context)
        {
            _moduleId = moduleId;
            _steps = steps;
            _context = context;
        }

        public IModuleContext AndRemoveIt()
        {
            var beginStepId = new StepId(_moduleId.ParentType, typeof(BeginModuleStep), _moduleId.TargetType);
            int moduleBegin = _steps.FindIndex(step => step.Id.Equals(beginStepId));

            if (moduleBegin == -1)
                throw new FatNetLibException($"Module {_moduleId.TargetType} not found");

            var endStepId = new StepId(_moduleId.ParentType, typeof(EndModuleStep), _moduleId.TargetType);
            int moduleEnd = _steps.FindIndex(step => step.Id.Equals(endStepId));

            if (moduleEnd == -1)
                throw new FatNetLibException($"Module {_moduleId.TargetType} begin is present, but end not found");

            _steps.RemoveRange(moduleBegin, moduleEnd - moduleBegin + 1);
            return _context;
        }

        public IModuleContext AndReplaceOld(ModuleId oldModuleId)
        {
            int oldModuleBegin = _steps.FindIndex(step => step.Id.Equals(
                new StepId(
                    oldModuleId.ParentType,
                    typeof(BeginModuleStep),
                    oldModuleId.TargetType)));
            new FindModuleContext(oldModuleId, _steps, _context).AndRemoveIt();

            var beginStepId = new StepId(_moduleId.ParentType, typeof(BeginModuleStep), _moduleId.TargetType);
            int moduleBegin = _steps.FindIndex(step => step.Id.Equals(beginStepId));

            if (moduleBegin == -1)
                throw new FatNetLibException($"Module {_moduleId.TargetType} not found");

            var endStepId = new StepId(_moduleId.ParentType, typeof(EndModuleStep), _moduleId.TargetType);
            int moduleEnd = _steps.FindIndex(step => step.Id.Equals(endStepId));

            if (moduleEnd == -1)
                throw new FatNetLibException($"Module {_moduleId.TargetType} begin is present, but end not found");

            IEnumerable<IModuleStep> moduleSteps = _steps.GetRange(moduleBegin, moduleEnd - moduleBegin + 1);

            _steps.RemoveRange(moduleBegin, moduleEnd - moduleBegin + 1);

            _steps.InsertRange(oldModuleBegin, moduleSteps);

            return _context;
        }
    }
}
