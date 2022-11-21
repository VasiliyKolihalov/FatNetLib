using System;

namespace Kolyhalov.FatNetLib.Core.Modules.Steps
{
    public class StepId
    {
        public StepId(Type parentModuleType, Type stepType, object inModuleId)
        {
            ParentModuleType = parentModuleType;
            StepType = stepType;
            InModuleId = inModuleId;
        }

        public Type ParentModuleType { get; }

        public Type StepType { get; }

        public object InModuleId { get; }

        private bool Equals(StepId other)
        {
            return StepType == other.StepType
                   && ParentModuleType == other.ParentModuleType
                   && InModuleId.Equals(other.InModuleId);
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((StepId)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(StepType, ParentModuleType, InModuleId);
        }

        public override string ToString()
        {
            return $"StepId(ParentModuleType: {ParentModuleType}, StepType: {StepType}, InModuleId: {InModuleId})";
        }
    }
}
