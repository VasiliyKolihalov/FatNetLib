using System;
using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Modules.Steps;

namespace Kolyhalov.FatNetLib.Core.Modules
{
    public class FindModuleModuleContext
    {
        private readonly Type _targetModuleType;
        private readonly List<IModuleStep> _steps;
        private readonly ModuleContext _context;

        public FindModuleModuleContext(Type targetModuleType, List<IModuleStep> steps, ModuleContext context)
        {
            _targetModuleType = targetModuleType;
            _steps = steps;
            _context = context;
        }

        public IModuleContext AndRemoveModule(Type moduleType)
        {
            var beginStepId = new StepId(_targetModuleType, typeof(BeginModuleStep), moduleType);
            int moduleBegin = _steps.FindIndex(step => step.Id.Equals(beginStepId));

            if (moduleBegin == -1)
                throw new FatNetLibException($"Module {moduleType} not found");

            var endStepId = new StepId(_targetModuleType, typeof(EndModuleStep), moduleType);
            int moduleEnd = _steps.FindIndex(step => step.Id.Equals(endStepId));

            if (moduleEnd == -1)
                throw new FatNetLibException($"Module {moduleType} begin is present, but end found");

            _steps.RemoveRange(moduleBegin, moduleEnd - moduleBegin + 1);
            return _context;
        }
    }
}
