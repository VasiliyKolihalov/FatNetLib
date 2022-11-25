using System.Collections.Generic;
using Kolyhalov.FatNetLib.Core.Exceptions;
using Kolyhalov.FatNetLib.Core.Modules.Steps;

namespace Kolyhalov.FatNetLib.Core.Modules
{
    public class FindStepModuleContext
    {
        private readonly StepId _targetStep;
        private readonly List<IModuleStep> _steps;
        private readonly ModuleContext _context;

        public FindStepModuleContext(StepId targetStep, List<IModuleStep> steps, ModuleContext context)
        {
            _targetStep = targetStep;
            _context = context;
            _steps = steps;
        }

        public IModuleContext AndMoveBeforeStep(StepId stepId)
        {
            int movingStepIndex = _steps.FindIndex(step => step.Id.Equals(_targetStep));
            if (movingStepIndex == -1)
                throw new FatNetLibException($"Step with id {_targetStep} not found");
            IModuleStep movingStep = _steps[movingStepIndex];

            int beforeStepIndex = _steps.FindIndex(step => step.Id.Equals(stepId));
            if (beforeStepIndex == -1)
                throw new FatNetLibException($"Step with id {stepId} not found");
            IModuleStep beforeStep = _steps[beforeStepIndex];

            _steps.Remove(movingStep);
            movingStep = movingStep.CopyWithNewId(new StepId(
                beforeStep.Id.ParentModuleType,
                movingStep.Id.StepType,
                movingStep.Id.Qualifier));
            beforeStepIndex = movingStepIndex < beforeStepIndex ? beforeStepIndex - 1 : beforeStepIndex;
            _steps.Insert(beforeStepIndex, movingStep);

            return _context;
        }

        public IModuleContext AndMoveAfterStep(StepId stepId)
        {
            int movingStepIndex = _steps.FindIndex(step => step.Id.Equals(_targetStep));
            if (movingStepIndex == -1)
                throw new FatNetLibException($"Step with id {_targetStep} not found");
            IModuleStep movingStep = _steps[movingStepIndex];

            int afterStepIndex = _steps.FindIndex(step => step.Id.Equals(stepId));
            if (afterStepIndex == -1)
                throw new FatNetLibException($"Step with id {stepId} not found");
            IModuleStep afterStep = _steps[afterStepIndex];

            _steps.Remove(movingStep);
            movingStep = movingStep.CopyWithNewId(new StepId(
                afterStep.Id.ParentModuleType,
                movingStep.Id.StepType,
                movingStep.Id.Qualifier));
            afterStepIndex = movingStepIndex < afterStepIndex ? afterStepIndex - 1 : afterStepIndex;
            _steps.Insert(afterStepIndex + 1, movingStep);

            return _context;
        }

        public IModuleContext AndReplaceOldStep(StepId stepId)
        {
            int movingStepIndex = _steps.FindIndex(step => step.Id.Equals(_targetStep));
            if (movingStepIndex == -1)
                throw new FatNetLibException($"Step with id {_targetStep} not found");
            IModuleStep movingStep = _steps[movingStepIndex];

            int replacingStepIndex = _steps.FindIndex(step => step.Id.Equals(stepId));
            if (replacingStepIndex == -1)
                throw new FatNetLibException($"Step with id {stepId} not found");
            IModuleStep replacingStep = _steps[replacingStepIndex];

            _steps.Remove(movingStep);
            _steps.Remove(replacingStep);
            movingStep = movingStep.CopyWithNewId(new StepId(
                replacingStep.Id.ParentModuleType,
                movingStep.Id.StepType,
                movingStep.Id.Qualifier));
            replacingStepIndex = movingStepIndex < replacingStepIndex ? replacingStepIndex - 1 : replacingStepIndex;
            _steps.Insert(replacingStepIndex, movingStep);

            return _context;
        }

        public IModuleContext AndRemoveIt()
        {
            int removingStepIndex = _steps.FindIndex(step => step.Id.Equals(_targetStep));
            if (removingStepIndex == -1)
                throw new FatNetLibException($"Step with id {_targetStep} not found");
            _steps.RemoveAt(removingStepIndex);
            return _context;
        }
    }
}
