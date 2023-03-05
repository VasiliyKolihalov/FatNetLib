using System;
using Kolyhalov.FatNetLib.Core.Utils;

namespace Kolyhalov.FatNetLib.Core.Modules.Steps
{
    public class StepId
    {
        public StepId(ModuleId parentModuleId, Type stepType, object qualifier)
        {
            ParentModuleId = parentModuleId;
            StepType = stepType;
            Qualifier = stepType == typeof(PutDependencyStep) && qualifier is Type typeQualifier
                ? typeQualifier.ToDependencyId()
                : qualifier;
        }

        public ModuleId ParentModuleId { get; }

        public Type StepType { get; }

        public object Qualifier { get; }

        public static readonly object EmptyQualifier = new object();

        public override string ToString()
        {
            return $"StepId(ParentModuleType: {ParentModuleId}, StepType: {StepType}, Qualifier: {Qualifier})";
        }

        protected bool Equals(StepId other)
        {
            return ParentModuleId.Equals(other.ParentModuleId)
                   && StepType == other.StepType
                   && Qualifier.Equals(other.Qualifier);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((StepId)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ParentModuleId, StepType, Qualifier);
        }
    }
}
