using System;

namespace Kolyhalov.FatNetLib.Core.Modules
{
    public class ModuleId
    {
        public ModuleId(Type parentType, Type targetType)
        {
            ParentType = parentType;
            TargetType = targetType;
        }

        public Type ParentType { get; }

        public Type TargetType { get; }
    }
}
