using System;

namespace Kolyhalov.FatNetLib.Core.Modules.Steps
{
    public class StepId
    {
        public StepId(Type parentModuleType, Type stepType, object qualifier)
        {
            ParentModuleType = parentModuleType;
            StepType = stepType;
            Qualifier = qualifier;
        }

        public Type ParentModuleType { get; }

        public Type StepType { get; }

        public object Qualifier { get; }

        private bool Equals(StepId other)
        {
            return StepType == other.StepType
                   && ParentModuleType == other.ParentModuleType
                   && Qualifier.Equals(other.Qualifier);
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((StepId)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(StepType, ParentModuleType, Qualifier);
        }

        public override string ToString()
        {
            return $"StepId(ParentModuleType: {ParentModuleType}, StepType: {StepType}, Qualifier: {Qualifier})";
        }
    }
}
