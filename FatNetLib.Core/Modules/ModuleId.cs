using System;
using System.Collections.Generic;
using System.Linq;

namespace Kolyhalov.FatNetLib.Core.Modules
{
    public class ModuleId
    {
        public ModuleId(params Type[] segments)
        {
            Segments = segments;
        }

        public IReadOnlyList<Type> Segments { get; }

        public static ModuleId operator /(ModuleId firstSegments, ModuleId lastSegments)
        {
            IEnumerable<Type> concatSegments = firstSegments.Segments.Concat(lastSegments.Segments);
            return new ModuleId(concatSegments.ToArray());
        }

        public static ModuleId operator /(ModuleId firstSegments, Type lastSegment)
        {
            IEnumerable<Type> concatSegments = firstSegments.Segments.AsEnumerable().Append(lastSegment);
            return new ModuleId(concatSegments.ToArray());
        }

        public override string ToString()
        {
            return $"ModuleId({string.Join(" / ", Segments)})";
        }

        protected bool Equals(ModuleId other)
        {
            return Segments.SequenceEqual(other.Segments);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ModuleId)obj);
        }

        public override int GetHashCode()
        {
            return Segments.GetHashCode();
        }

        public static class Pointers
        {
            public static readonly ModuleId ThisModule = new ModuleId(typeof(ThisModulePointer));
            public static readonly ModuleId ParentModule = new ModuleId(typeof(ParentModulePointer));
            public static readonly ModuleId RootModule = new ModuleId(typeof(RootModulePointer));

            public static class ThisModulePointer
            {
            }

            public static class ParentModulePointer
            {
            }

            public static class RootModulePointer
            {
            }
        }
    }
}
